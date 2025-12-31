namespace Craftdig.Server;

[Server]
public class ServerKicker(AppLog log, ServerSockets sockets)
{
    public void Tick()
    {
        var now = DateTime.UtcNow;
        int count = 0;
        int kicked = 0;

        sockets.ForEach(ns =>
        {
            if (ns.Ent.IsAuthenticated())
                return;

            count++;

            var dt = now - ns.Ent.ConnectedTime();
            if (dt.TotalSeconds < 3)
                return;

            log.Warn("Kicking socket {0}", ns.Ent.Tag());
            ns.Disconnect();
            kicked++;
        });

        if (count > 0)
            log.Debug("There are {0} unauthenticated sockets", count - kicked);
    }
}
