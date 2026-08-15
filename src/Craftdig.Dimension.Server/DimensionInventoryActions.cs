namespace Craftdig;

[Dimension]
public class DimensionInventoryActions(
    DimensionInventoryOperations operations,
    DimensionInventoryActionQueue queue)
{
    public void Tick()
    {
        while (queue.TryDequeue(out var request))
            Apply(request.Socket, request.Command);
    }

    private void Apply(NetSocket socket, InventoryActionCommand command)
    {
        if (!socket.Connected || socket.SocketPlayer == default)
            return;

        if (command.Sequence == socket.LastInventoryActionSequence)
        {
            socket.Send(new InventoryActionResultCommand(
                command.Sequence,
                socket.LastInventoryActionStatus,
                socket.LastInventoryActionRevision));
            return;
        }

        if (command.Sequence != socket.LastInventoryActionSequence + 1)
        {
            socket.Disconnect();
            return;
        }

        var player = socket.SocketPlayer;
        var result = operations.Apply(player, command.Operation);
        if (result == InventoryApplyResult.Changed)
            player.InventoryRevision++;

        var status = result == InventoryApplyResult.Rejected
            ? InventoryActionStatus.Rejected
            : InventoryActionStatus.Accepted;
        socket.LastInventoryActionSequence = command.Sequence;
        socket.LastInventoryActionStatus = status;
        socket.LastInventoryActionRevision = player.InventoryRevision;
        socket.Send(new InventoryActionResultCommand(command.Sequence, status, player.InventoryRevision));
    }
}
