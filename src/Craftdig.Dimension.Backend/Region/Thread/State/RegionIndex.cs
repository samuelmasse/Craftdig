namespace Craftdig.Dimension.Backend;

public class RegionIndex
{
    private readonly RegionIndexEntry[] allocs = new RegionIndexEntry[RegionVolume];

    public Span<byte> Bytes => MemoryMarshal.AsBytes(allocs.AsSpan());

    public ReadOnlySpan<RegionIndexEntry> Span => allocs;

    public ref RegionIndexEntry this[Vector3i offset] => ref allocs[Index(offset)];

    public int Index(Vector3i offset) =>
        offset.Z * (RegionSize * RegionSize) + offset.Y * RegionSize + offset.X;
}
