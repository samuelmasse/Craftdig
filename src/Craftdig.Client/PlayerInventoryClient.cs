namespace Craftdig.Client;

[Player]
public class PlayerInventoryClient(
    PlayerSocket socket,
    PlayerInventoryActions actions)
{
    private readonly ConcurrentQueue<InventoryActionResultCommand> results = [];

    public void Receive(InventoryActionResultCommand result) => results.Enqueue(result);

    public void Frame()
    {
        while (results.TryDequeue(out var result))
        {
            actions.Acknowledge(
                result.Sequence,
                result.Status == InventoryActionStatus.Accepted,
                result.Revision);
        }

        while (actions.TryTakeUnsent(out uint sequence, out var operation))
            socket.Send(new InventoryActionCommand(sequence, operation));
    }
}
