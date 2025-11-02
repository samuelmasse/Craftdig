namespace Crafthoe.Server;

[Dimension]
public class DimensionMovePlayerReceiver : DimensionReceiver
{
    public override void Receive(NetSocket ns, NetMessage msg)
    {
        ref var pending = ref ns.Ent.SocketPlayer().PendingMovement();
        pending ??= [];
        pending.Enqueue(MemoryMarshal.AsRef<MovementStep>(msg.Data));
    }
}
