namespace Craftdig;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct PresenceRoundChunkCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.PresenceRoundChunk;
}
