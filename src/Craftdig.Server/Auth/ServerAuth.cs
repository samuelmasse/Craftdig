namespace Craftdig.Server;

[Server]
public class ServerAuth(
    AppLog log,
    ServerClientLimits clientLimits,
    ServerAuthGuards guards,
    ServerIdentityTrust ticketValidator,
    ServerIdentitySessionEvents identitySessionEvents)
{
    public void SendNonce(NetSocket socket)
    {
        if (!socket.IsTransportSecure)
        {
            Reject(socket, "tried to use Identity authentication without TLS");
            return;
        }

        Nonce256 nonce;
        lock (guards.AdmissionGate)
        {
            if (!socket.Connected)
                return;

            if (socket.IsAuthenticated && socket.IdentitySession == null)
            {
                Reject(socket, "tried to replace an unverified no-auth session");
                return;
            }

            if (socket.IdentitySession is { } current && current.Ticket.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                Reject(socket, "tried to refresh an expired Identity session");
                return;
            }

            if (socket.AuthChallenge != null)
            {
                Reject(socket, "tried to get a second authentication nonce");
                return;
            }

            nonce = Nonce256.CreateRandom();
            socket.AuthChallenge = new(nonce);
        }

        Span<byte> body = stackalloc byte[ReadyAuthCommandCodec.Size];
        ReadyAuthCommandCodec.TryWrite(body, nonce, out int written);
        socket.SendRaw<ReadyAuthCommand>(body[..written]);
    }

    public void AuthenticateTicket(NetSocket socket, ReadOnlySpan<byte> data)
    {
        if (!socket.IsTransportSecure)
        {
            Reject(socket, "tried to submit an Identity ticket without TLS");
            return;
        }

        if (!CompleteAuthCommandCodec.TryRead(data, out var rawTicket, out var signature))
        {
            log.Warn("Socket {0} sent a malformed authentication response ({1} bytes)", socket.Tag, data.Length);
            socket.Disconnect();
            return;
        }

        if (CaptureChallenge(socket) is not { } exchange)
            return;
        if (ValidateTicket(socket, rawTicket, exchange.Challenge, signature) is not { } ticket)
            return;
        if (InstallSession(socket, ticket, exchange.Challenge, exchange.Previous) is not { } session)
            return;

        if (!socket.TrySendRaw<ResultAuthCommand>([]))
        {
            RollBackFailedResult(socket, session.Installed, exchange.Previous, session.Registration);
            return;
        }

        session.Registration.Activate();
        identitySessionEvents.Signal();
        if (!session.Refresh)
            clientLimits.Pulse();
    }

    private (ServerAuthChallenge Challenge, ServerIdentitySessionSnapshot? Previous)? CaptureChallenge(NetSocket socket)
    {
        lock (guards.AdmissionGate)
        {
            if (socket.AuthChallenge is not { } pending)
            {
                Reject(socket, "tried to authenticate without a nonce");
                return null;
            }

            if (socket.IsAuthenticated && socket.IdentitySession == null)
            {
                Reject(socket, "tried to replace an unverified no-auth session");
                return null;
            }

            return (pending, socket.IdentitySession);
        }
    }

    private ValidatedIdentityTicket? ValidateTicket(
        NetSocket socket,
        ReadOnlySpan<byte> rawTicket,
        ServerAuthChallenge challenge,
        P256Signature signature)
    {
        var ticket = ticketValidator.Validate(rawTicket, out var validationFailure);
        if (ticket == null)
        {
            log.Warn("Socket {0} sent an invalid Identity ticket (stage={1})", socket.Tag, validationFailure);
            socket.Disconnect();
            return null;
        }

        if (!VerifyProofOfPossession(ticket, challenge.Nonce, signature))
        {
            socket.Disconnect();
            return null;
        }

        return ticket;
    }

    private (ServerIdentitySessionSnapshot Installed, ServerPresenceRegistration Registration, bool Refresh)? InstallSession(
        NetSocket socket,
        ValidatedIdentityTicket ticket,
        ServerAuthChallenge challenge,
        ServerIdentitySessionSnapshot? previous)
    {
        lock (guards.AdmissionGate)
        {
            if (!socket.Connected || !ReferenceEquals(socket.AuthChallenge, challenge) ||
                !ReferenceEquals(socket.IdentitySession, previous))
            {
                Reject(socket, "authentication state changed before completion");
                return null;
            }

            bool refresh = previous != null;
            if (refresh)
            {
                if (!ValidRefresh(previous!, ticket, out string? refreshFailure))
                {
                    Reject(socket, $"sent an invalid Identity refresh ticket: {refreshFailure}");
                    return null;
                }
            }
            else if (socket.IsAuthenticated || guards.GuardAllowlist(socket, ticket) ||
                guards.GuardDuplicatePlayer(socket, ticket.PlayerId) || guards.GuardMaxPlayers(socket))
            {
                return null;
            }

            var installed = new ServerIdentitySessionSnapshot(
                socket.ConnectionGeneration,
                previous == null ? 1 : previous.TicketRevision + 1,
                ticket);
            if (!identitySessionEvents.TryPublishIdentity(socket, installed, refresh, out var registration))
            {
                Reject(socket, "could not register its Identity session with presence");
                return null;
            }

            socket.IdentitySession = installed;
            socket.PresenceConnection = registration.Connection;
            socket.AuthenticatedUid = ticket.PlayerId.ToString("D");
            socket.AuthenticatedUsername = ticket.Username;
            socket.IsAuthenticated = true;
            socket.AuthChallenge = null;
            socket.Tag = ticket.Username;
            return (installed, registration, refresh);
        }
    }

    public bool AuthorizeGameplay(NetSocket socket)
    {
        ServerIdentitySessionSnapshot? expired = null;
        lock (guards.AdmissionGate)
        {
            if (!socket.IsAuthenticated)
                return false;

            if (socket.IdentitySession is not { } identity || identity.Ticket.ExpiresAt > DateTimeOffset.UtcNow)
                return true;

            socket.IsAuthenticated = false;
            expired = identity;
        }

        log.Warn("Socket {0} Identity ticket revision {1} expired", socket.Tag, expired.TicketRevision);
        socket.Disconnect();
        return false;
    }

    public bool ExpireIdentitySession(
        NetSocket socket,
        ServerIdentitySessionSnapshot expected,
        DateTimeOffset now)
    {
        lock (guards.AdmissionGate)
        {
            if (!socket.IsAuthenticated || !ReferenceEquals(socket.IdentitySession, expected) ||
                expected.Ticket.ExpiresAt > now)
                return false;

            socket.IsAuthenticated = false;
        }

        log.Warn("Socket {0} Identity ticket revision {1} expired", socket.Tag, expected.TicketRevision);
        socket.Disconnect();
        return true;
    }

    private bool VerifyProofOfPossession(
        ValidatedIdentityTicket ticket,
        Nonce256 nonce,
        P256Signature signature)
    {
        var digest = AuthenticationDigest.Compute(ticket.ServerContext.ComputeHash(), nonce, ticket.TicketHash);
        using var publicKey = ticket.PublicKey.CreateEcdsa();
        return signature.VerifyHash(publicKey, digest);
    }

    private bool ValidRefresh(
        ServerIdentitySessionSnapshot previous,
        ValidatedIdentityTicket replacement,
        [NotNullWhen(false)] out string? failure)
    {
        var current = previous.Ticket;
        if (current.ExpiresAt <= DateTimeOffset.UtcNow)
            return Invalid("current ticket expired", out failure);
        if (replacement.IssuedAt <= current.IssuedAt)
            return Invalid("replacement is not newer", out failure);
        if (replacement.PlayerId != current.PlayerId)
            return Invalid("player ID changed", out failure);
        if (replacement.SessionId != current.SessionId)
            return Invalid("session ID changed", out failure);
        if (replacement.PublicKey != current.PublicKey)
            return Invalid("proof key changed", out failure);
        if (replacement.ServerContext != current.ServerContext)
            return Invalid("server context changed", out failure);

        failure = null;
        return true;
    }

    private static bool Invalid(string reason, out string? failure)
    {
        failure = reason;
        return false;
    }

    private void RollBackFailedResult(
        NetSocket socket,
        ServerIdentitySessionSnapshot installed,
        ServerIdentitySessionSnapshot? previous,
        ServerPresenceRegistration registration)
    {
        lock (guards.AdmissionGate)
        {
            if (ReferenceEquals(socket.IdentitySession, installed))
            {
                socket.IdentitySession = previous;
                socket.IsAuthenticated = false;
                socket.AuthChallenge = null;
                if (previous == null)
                {
                    socket.AuthenticatedUid = null;
                    socket.AuthenticatedUsername = null;
                }
            }
        }

        registration.Connection.Cancel();
        log.Warn("Socket {0} could not enqueue the authentication result", socket.Tag);
        socket.Disconnect();
    }

    private void Reject(NetSocket socket, string reason)
    {
        log.Warn("Socket {0} {1}", socket.Tag, reason);
        socket.Disconnect();
    }
}
