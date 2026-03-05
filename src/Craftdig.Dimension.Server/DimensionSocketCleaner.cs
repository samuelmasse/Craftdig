namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionSocketCleaner(AppLog log, DimensionSockets sockets, DimensionPlayerBag playerBag, DimensionRigidBag rigidBag)
{
    private readonly List<NetSocket> remove = [];

    public void Tick()
    {
        foreach (var ns in sockets.Span)
        {
            if (!ns.Connected)
                remove.Add(ns);
        }

        foreach (var ns in remove)
        {
            playerBag.Remove(ns.SocketPlayer);
            rigidBag.Remove(ns.SocketPlayer);
            sockets.Remove(ns);

            log.Info("Player {0} left", ns.SocketPlayer.Tag);
        }

        remove.Clear();
    }
}
