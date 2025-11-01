namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential)]
public struct ChunkUpdateBlockEntry
{
    public static readonly int Size = Marshal.SizeOf<ChunkUpdateBlockEntry>();

    public int Value;
    public int Count;
}
