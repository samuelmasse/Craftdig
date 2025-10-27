namespace Crafthoe.Server;

[Dimension]
public class DimensionPositionStreamer(DimensionSockets sockets, WorldPositionUpdateWrapper positionUpdateWrapper)
{
    public void Tick()
    {
        foreach (var ns in sockets.Span)
        {
            ns.Send(positionUpdateWrapper.Wrap(ns.Ent.SocketPlayer().Position()));
        }
    }
}
