namespace Craftdig.Server;

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

            if (!TryReadName(data, out string? name))
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
            socket.AuthenticatedUsername = uid;
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

    private bool TryReadName(ReadOnlySpan<byte> data, [NotNullWhen(true)] out string? name)
    {
        name = null;
        if (data.Length is < 1 or > 35)
            return false;

        foreach (byte value in data)
        {
            if (value is < 0x21 or > 0x7e)
                return false;
        }

        name = Encoding.ASCII.GetString(data);
        return true;
    }

    private void Reject(NetSocket socket, string reason)
    {
        log.Warn("Socket {0} {1}", socket.Tag, reason);
        socket.Disconnect();
    }
}
