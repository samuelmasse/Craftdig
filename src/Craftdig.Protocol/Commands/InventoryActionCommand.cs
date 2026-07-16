namespace Craftdig.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct InventoryActionCommand(uint sequence, InventoryOperation operation) : ICommand
{
    public readonly uint Sequence = sequence;
    public readonly InventoryOperation Operation = operation;

    public static ushort CommandId => (ushort)Commands.InventoryAction;
}
