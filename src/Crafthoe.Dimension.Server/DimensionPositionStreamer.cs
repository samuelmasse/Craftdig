namespace Crafthoe.Dimension.Server;

[Dimension]
public class DimensionPositionStreamer(DimensionSockets sockets)
{
    public void Tick()
    {
        foreach (var ns in sockets.Span)
        {
            var cmd = new PositionUpdateCommand()
            {
                Position = ns.Ent.SocketPlayer().Position(),
                IsFlying = ns.Ent.SocketPlayer().IsFlying(),
                IsSprinting = ns.Ent.SocketPlayer().IsSprinting()
            };

            ns.Send(new((int)ClientCommand.PositionUpdate, MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref cmd, 1))));
        }
    }
}
