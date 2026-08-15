namespace Craftdig;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct PresenceProofBatchCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.PresenceProofBatch;
}
