namespace Craftdig;

public class RegionIndex
{
    private readonly RegionIndexEntry[] allocs = new RegionIndexEntry[RegionVolume];

    public Span<byte> Bytes => MemoryMarshal.AsBytes(allocs.AsSpan());

    public ReadOnlySpan<RegionIndexEntry> Span => allocs;

    public ref RegionIndexEntry this[Vec3i offset] => ref allocs[Index(offset)];

    public int Index(Vec3i offset) =>
        offset.Z * (RegionSize * RegionSize) + offset.Y * RegionSize + offset.X;
}
