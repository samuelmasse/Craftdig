namespace Crafthoe.Dimension;

[StructLayout(LayoutKind.Sequential)]
public struct RegionIndexEntry
{
    public static readonly int Size = Marshal.SizeOf<RegionIndexEntry>();

    public byte Bucket;
    public ushort Offset;
    public ushort Count;
}
