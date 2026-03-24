namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionPlayerSocketsCleaner(
    AppLog log,
    WorldPlayerSockets worldSockets,
    WorldPlayerSlots playerSlots,
    DimensionSockets sockets)
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

            playerSlots.Return(ns.PlayerSlot);

            worldSockets.Remove(ns);
            sockets.Remove(ns);

            ns.SocketPlayer.Dispose();
            ns.SocketWorldPlayer.Dispose();

            log.Info("Player {0} left", tag);
        }

        remove.Clear();
    }
}
