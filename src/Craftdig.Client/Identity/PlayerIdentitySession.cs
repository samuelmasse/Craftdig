namespace Craftdig;

[Player]
public sealed class PlayerIdentitySession : IDisposable
{
    private readonly Lock gate = new();
    private ECDsa? signingKey;
    private ValidatedIdentityTicket? currentTicket;

    private PlayerIdentitySession(
        bool identityEnabled,
        ServerContext? serverContext,
        SessionId sessionId,
        ECDsa? signingKey,
        P256PublicKey publicKey)
    {
        IdentityEnabled = identityEnabled;
        ServerContext = serverContext;
        SessionId = sessionId;
        this.signingKey = signingKey;
        PublicKey = publicKey;
    }

    public readonly bool IdentityEnabled;
    public readonly ServerContext? ServerContext;
    public readonly SessionId SessionId;
    public readonly P256PublicKey PublicKey;

    public static PlayerIdentitySession CreateAuthenticated(ServerContext serverContext)
    {
        var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var parameters = key.ExportParameters(false);

        if (parameters.Q.X == null || parameters.Q.Y == null ||
            !P256PublicKey.TryCreate(parameters.Q.X, parameters.Q.Y, out var publicKey))
        {
            key.Dispose();
            throw new CryptographicException("Unable to export a canonical P-256 session key.");
        }

        return new(true, serverContext, SessionId.CreateRandom(), key, publicKey);
    }

    public static PlayerIdentitySession CreateUnverified(ServerContext? serverContext = null) =>
        new(false, serverContext, default, null, default);

    public ValidatedIdentityTicket? GetCurrentTicket()
    {
        lock (gate)
            return currentTicket;
    }

    public void InstallTicket(ValidatedIdentityTicket ticket)
    {
        lock (gate)
        {
            ThrowIfDisposed();
            GuardTicketBinding(ticket);

            if (currentTicket != null)
            {
                if (ticket.PlayerId != currentTicket.PlayerId ||
                    ticket.SessionId != currentTicket.SessionId ||
                    ticket.PublicKey != currentTicket.PublicKey ||
                    ticket.IssuedAt <= currentTicket.IssuedAt)
                    throw new InvalidOperationException("The refreshed ticket changed the connection identity or is not newer.");
            }

            currentTicket = ticket;
        }
    }

    public P256Signature SignAuthentication(ValidatedIdentityTicket ticket, Nonce256 nonce)
    {
        lock (gate)
        {
            ThrowIfDisposed();
            GuardTicketBinding(ticket);

            var digest = AuthenticationDigest.Compute(
                ServerContext!.ComputeHash(),
                nonce,
                ticket.TicketHash);
            return P256Signature.SignHash(signingKey!, digest);
        }
    }

    public bool TrySignPresence(
        Hash256 roundHash,
        Hash256 expectedTicketHash,
        out P256Signature signature)
    {
        lock (gate)
        {
            if (signingKey == null || currentTicket == null || ServerContext == null ||
                currentTicket.TicketHash != expectedTicketHash ||
                currentTicket.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                signature = default;
                return false;
            }

            var digest = PresenceProofDigest.Compute(ServerContext.ComputeHash(), roundHash, expectedTicketHash);
            signature = P256Signature.SignHash(signingKey, digest);
            return true;
        }
    }

    public void Dispose()
    {
        lock (gate)
        {
            signingKey?.Dispose();
            signingKey = null;
            currentTicket = null;
        }
    }

    private void GuardTicketBinding(ValidatedIdentityTicket ticket)
    {
        if (!IdentityEnabled || signingKey == null || ServerContext == null)
            throw new InvalidOperationException("This development connection has no verifiable Identity session.");

        if (ticket.SessionId != SessionId || ticket.PublicKey != PublicKey || ticket.ServerContext != ServerContext)
            throw new InvalidDataException("The Identity ticket is not bound to this connection session.");
    }

    private void ThrowIfDisposed()
    {
        if (signingKey == null)
            throw new ObjectDisposedException(nameof(PlayerIdentitySession));
    }
}
