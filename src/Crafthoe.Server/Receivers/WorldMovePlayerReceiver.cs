namespace Crafthoe.Server;

[World]
public class WorldMovePlayerReceiver
{
    public void Receive(NetSocket ns, NetMessage msg)
    {
        ref var pending = ref ns.Ent.SocketPlayer().PendingMovement();
        pending ??= [];
        pending.Enqueue(MemoryMarshal.AsRef<MovementStep>(msg.Data));
    }
}
