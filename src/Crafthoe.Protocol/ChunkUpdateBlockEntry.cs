namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ChunkUpdateBlockEntry
{
    public static readonly int Size = Marshal.SizeOf<ChunkUpdateBlockEntry>();

    public int Value;
    public int Count;
}
