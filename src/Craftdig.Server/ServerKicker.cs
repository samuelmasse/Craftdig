namespace Craftdig;

[Server]
public class ServerKicker(Log log, ServerSockets sockets, ServerAuth auth)
{
    private readonly List<(NetSocket Socket, ServerIdentitySessionSnapshot Identity)> expiryCandidates = [];

    public void Tick()
    {
        var now = DateTimeOffset.UtcNow;
        int count = 0;
        int unauthenticatedKicked = 0;
        int expiredKicked = 0;
        expiryCandidates.Clear();

        sockets.ForEach(ns =>
        {
            if (ns.IsAuthenticated)
            {
                if (ns.IdentitySession is { } identity && identity.Ticket.ExpiresAt <= now)
                    expiryCandidates.Add((ns, identity));
                return;
            }

            count++;

            var dt = now - ns.ConnectedTime;
            if (dt.TotalSeconds < 30)
                return;

            log.Warn("Kicking socket {0}", ns.Tag);
            ns.Disconnect();
            unauthenticatedKicked++;
        });

        foreach (var candidate in expiryCandidates)
        {
            if (auth.ExpireIdentitySession(candidate.Socket, candidate.Identity, now))
                expiredKicked++;
        }

        if (count > 0)
            log.Debug("There are {0} unauthenticated sockets", count - unauthenticatedKicked);
        if (expiredKicked > 0)
            log.Info("Kicked {0} sockets with expired Identity tickets", expiredKicked);
    }
}
