namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionSocketCleaner(DimensionSockets sockets)
{
    public void Remove(NetSocket ns)
    {
        sockets.Remove(ns);
        ns.SocketPlayer.Dispose();
    }
}
