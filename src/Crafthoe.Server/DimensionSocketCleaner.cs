namespace Crafthoe.Server;

[Dimension]
public class DimensionSocketCleaner(DimensionSockets sockets, DimensionPlayerBag playerBag)
{
    private readonly List<NetSocket> remove = [];

    public void Tick()
    {
        foreach (var ns in sockets.Span)
        {
            if (!ns.Raw.Connected)
                remove.Add(ns);
        }

        foreach (var ns in remove)
        {
            playerBag.Remove((EntMut)ns.Ent.SocketPlayer());
            sockets.Remove(ns);
        }

        remove.Clear();
    }
}
