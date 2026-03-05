namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionPositionStreamer(DimensionSockets sockets)
{
    public void Tick()
    {
        foreach (var ns in sockets.Span)
        {
            ns.Send(new PositionUpdateCommand()
            {
                Position = ns.SocketPlayer.Position,
                Velocity = ns.SocketPlayer.Velocity,
                IsFlying = ns.SocketPlayer.IsFlying,
                IsSprinting = ns.SocketPlayer.IsSprinting
            });
        }
    }
}
