namespace Craftdig.Server;

[Dimension]
public class DimensionMovePlayerReceiver : DimensionReceiver<MovePlayerCommand>
{
    public override void Receive(NetSocket ns, MovePlayerCommand cmd)
    {
        ref var pending = ref ns.Ent.SocketPlayer().PendingMovement();
        pending ??= [];
        pending.Enqueue(cmd);
    }
}
