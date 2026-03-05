namespace Craftdig.Server;

[Dimension]
public class DimensionMovePlayerReceiver : DimensionReceiver<MovePlayerCommand>
{
    public override void Receive(NetSocket ns, MovePlayerCommand cmd)
    {
        var pending = ns.SocketPlayer.PendingMovement ??= [];
        pending.Enqueue(cmd);
    }
}
