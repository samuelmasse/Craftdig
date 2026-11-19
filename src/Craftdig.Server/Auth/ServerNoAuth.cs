namespace Craftdig;

[Server]
public class ServerNoAuth(
    Log log,
    ServerClientLimits clientLimits,
    ServerAuthGuards guards,
    ServerIdentitySessionEvents identitySessionEvents)
{
    public void Authenticate(NetSocket socket, ReadOnlySpan<byte> data)
    {
        ServerPresenceRegistration registration;
        lock (guards.AdmissionGate)
        {
            if (socket.IsAuthenticated)
            {
                Reject(socket, "tried to re-authenticate");
                return;
            }

            string? name = DevelopmentPlayerName.Decode(data);

            if (name == null)
            {
                Reject(socket, "sent an invalid no-auth development name");
                return;
            }

            string uid = $"#{name}#";
            guards.DisconnectSocketsWithSameUid(uid);
            if (guards.GuardMaxPlayers(socket))
                return;
            if (!identitySessionEvents.TryPublishUnverified(socket, out registration))
            {
                Reject(socket, "could not register its unverified session with presence");
                return;
            }

            socket.IdentitySession = null;
            socket.PresenceConnection = registration.Connection;
            socket.AuthenticatedUid = uid;
            socket.AuthenticatedUsername = name;
            socket.IsAuthenticated = true;
            socket.AuthChallenge = null;
            socket.Tag = uid;
            log.Info("Socket {0} no-auth authenticated as an unverified player", socket.Tag);
        }

        if (!socket.TrySendRaw<ResultAuthCommand>([]))
        {
            RollBack(socket, registration);
            return;
        }

        registration.Activate();
        identitySessionEvents.Signal();
        clientLimits.Pulse();
    }

    private void RollBack(NetSocket socket, ServerPresenceRegistration registration)
    {
        lock (guards.AdmissionGate)
        {
            socket.IdentitySession = null;
            socket.IsAuthenticated = false;
            socket.AuthChallenge = null;
            socket.AuthenticatedUid = null;
            socket.AuthenticatedUsername = null;
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
