namespace Craftdig.Server;

[Server]
public class ServerPlayerSocketsCleaner(AppLog log, ServerPlayerSockets sockets)
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
            var tag = ns.SocketWorldPlayer.Tag;
            sockets.Remove(ns);
            ns.DimensionScope.Get<DimensionSocketCleaner>().Remove(ns);
            ns.SocketWorldPlayer.Dispose();
            log.Info("Player {0} left", tag);
        }

        remove.Clear();
    }
}
