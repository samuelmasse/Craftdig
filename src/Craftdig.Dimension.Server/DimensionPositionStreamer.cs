namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionPositionStreamer(DimensionSockets sockets)
{
    public void Tick()
    {
        foreach (var ns in sockets.Span)
        {
            if (ns.PlayerSpawnPhase != PlayerSpawnPhase.Active)
                continue;

            ns.Send(new PositionUpdateCommand()
            {
                Position = ns.SocketPlayer.Position,
                Velocity = ns.SocketPlayer.Velocity,
                LookAt = ns.SocketPlayer.LookAt,
                IsFlying = ns.SocketPlayer.IsFlying,
                IsSprinting = ns.SocketPlayer.IsSprinting
            });
        }
    }
}
