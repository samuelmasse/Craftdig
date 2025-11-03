namespace Crafthoe.Dimension.Server;

[Dimension]
public class DimensionSocketCleaner(DimensionSockets sockets, DimensionPlayerBag playerBag, DimensionRigidBag rigidBag)
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
            playerBag.Remove(ns.Ent.SocketPlayer());
            rigidBag.Remove(ns.Ent.SocketPlayer());
            sockets.Remove(ns);
        }

        remove.Clear();
    }
}
