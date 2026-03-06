namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionSocketCleaner(AppLog log, DimensionSockets sockets)
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
            sockets.Remove(ns);
            log.Info("Player {0} left", ns.SocketPlayer.Tag);
            ns.SocketPlayer.Dispose();
        }

        remove.Clear();
    }
}
