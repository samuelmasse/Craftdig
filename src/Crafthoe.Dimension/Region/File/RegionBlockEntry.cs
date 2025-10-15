namespace Crafthoe.Dimension;

[StructLayout(LayoutKind.Sequential)]
public struct RegionBlockEntry
{
    public static readonly int Size = Marshal.SizeOf<RegionBlockEntry>();

    public int Value;
    public int Count;
}
