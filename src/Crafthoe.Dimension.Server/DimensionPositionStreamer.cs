namespace Crafthoe.Dimension.Server;

[Dimension]
public class DimensionPositionStreamer(DimensionSockets sockets)
{
    public void Tick()
    {
        foreach (var ns in sockets.Span)
        {
            ns.Send(new PositionUpdateCommand()
            {
                Position = ns.Ent.SocketPlayer().Position(),
                Velocity = ns.Ent.SocketPlayer().Velocity(),
                IsFlying = ns.Ent.SocketPlayer().IsFlying(),
                IsSprinting = ns.Ent.SocketPlayer().IsSprinting()
            });
        }
    }
}
