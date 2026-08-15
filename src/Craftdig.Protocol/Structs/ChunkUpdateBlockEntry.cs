namespace Craftdig;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct ChunkUpdateBlockEntry
{
    public int Value;
    public int Count;
}
