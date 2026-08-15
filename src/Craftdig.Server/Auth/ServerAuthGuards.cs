namespace Craftdig;

[Server]
public class ServerAuthGuards(Log log, ServerConfig config, ServerSockets sockets, SeverAllowlist allowlist)
{
    public readonly object AdmissionGate = new();

    public bool GuardMaxPlayers(NetSocket socket)
    {
        int count = 0;
        sockets.ForEach(candidate =>
        {
            if (candidate.Connected && candidate.IsAuthenticated)
                count++;
        });

        if (count < config.MaxPlayers)
            return false;

        log.Warn("Socket {0} rejected: server full ({1}/{2})", socket.Tag, count, config.MaxPlayers);
        socket.Disconnect();
        return true;
    }

    public bool GuardAllowlist(NetSocket socket, ValidatedIdentityTicket ticket)
    {
        if (allowlist.Allow(ticket.PlayerId))
            return false;

        log.Warn("Socket {0} tried to join but player {1} is not allowlisted", socket.Tag, ticket.PlayerId);
        socket.Disconnect();
        return true;
    }

    public bool GuardDuplicatePlayer(NetSocket socket, Guid playerId)
    {
        bool duplicate = false;
        sockets.ForEach(candidate =>
        {
            if (!ReferenceEquals(candidate, socket) && candidate.Connected && candidate.IsAuthenticated &&
                candidate.IdentitySession?.Ticket.PlayerId == playerId)
                duplicate = true;
        });

        if (!duplicate)
            return false;

        log.Warn("Socket {0} rejected: player {1} already has an active session", socket.Tag, playerId);
        socket.Disconnect();
        return true;
    }

    public void DisconnectSocketsWithSameUid(string uid)
    {
        sockets.ForEach(socket =>
        {
            if (socket.AuthenticatedUid == uid)
            {
                socket.Disconnect();
                log.Info("Socket {0} kicked due to a no-auth uid conflict", socket.Tag);
            }
        });
    }
}
