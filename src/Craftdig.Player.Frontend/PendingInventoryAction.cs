namespace Craftdig;

public struct PendingInventoryAction(uint sequence, InventoryOperation operation)
{
    public readonly uint Sequence = sequence;
    public readonly InventoryOperation Operation = operation;
    public bool Sent;
    public PendingInventoryActionStatus Status;
    public long Revision;
}
