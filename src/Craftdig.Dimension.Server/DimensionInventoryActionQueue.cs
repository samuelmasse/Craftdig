namespace Craftdig;

[Dimension]
public class DimensionInventoryActionQueue
{
    private readonly ConcurrentQueue<DimensionInventoryActionRequest> requests = [];

    public void Enqueue(NetSocket socket, InventoryActionCommand command) =>
        requests.Enqueue(new(socket, command));

    public bool TryDequeue(out DimensionInventoryActionRequest request) =>
        requests.TryDequeue(out request);
}
