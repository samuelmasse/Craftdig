namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ChunkUpdateBlockEntry
{
    public int Value;
    public int Count;
}
