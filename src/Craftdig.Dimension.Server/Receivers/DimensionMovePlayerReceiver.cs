namespace Craftdig;

[Dimension]
public class DimensionMovePlayerReceiver : DimensionReceiver<MovePlayerCommand>
{
    public override void Receive(NetSocket ns, MovePlayerCommand cmd)
    {
        ns.SocketPlayer.PendingMovement!.Enqueue(cmd);
    }
}
