namespace Craftdig;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct InventoryActionResultCommand(
    uint sequence,
    InventoryActionStatus status,
    long revision) : ICommand
{
    public readonly uint Sequence = sequence;
    public readonly InventoryActionStatus Status = status;
    public readonly long Revision = revision;

    public static ushort CommandId => (ushort)Commands.InventoryActionResult;
}
