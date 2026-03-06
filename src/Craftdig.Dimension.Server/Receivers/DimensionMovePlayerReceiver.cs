namespace Craftdig.Server;

[Dimension]
public class DimensionMovePlayerReceiver : DimensionReceiver<MovePlayerCommand>
{
    public override void Receive(NetSocket ns, MovePlayerCommand cmd)
    {
        var player = ns.SocketPlayer;
        var pending = player.PendingMovement ??= [];
        pending.Enqueue(cmd);
    }
}
