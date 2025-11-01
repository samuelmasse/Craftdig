namespace Crafthoe.Dimension.Server;

[Dimension]
public class DimensionPositionStreamer(DimensionSockets sockets, WorldPositionUpdateWrapper positionUpdateWrapper)
{
    public void Tick()
    {
        foreach (var ns in sockets.Span)
        {
            ns.Send(new((int)ClientCommand.PositionUpdate,
                positionUpdateWrapper.Wrap(ns.Ent.SocketPlayer().Position())));
        }
    }
}
