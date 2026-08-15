namespace Craftdig;

[Dimension]
public class DimensionInventoryActionReceiver(DimensionInventoryActionQueue queue) :
    DimensionReceiver<InventoryActionCommand>
{
    public override void Receive(NetSocket socket, InventoryActionCommand command) =>
        queue.Enqueue(socket, command);
}
