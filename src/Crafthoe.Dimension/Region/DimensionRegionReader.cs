namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRegionReader(
    WorldModuleIndices moduleIndices,
    DimensionBlocksRaw blocksRaw,
    DimensionRegions regions,
    DimensionRegionFileHandles regionFileHandles,
    DimensionRegionIndexLoader regionIndexLoader,
    DimensionRegionBuckets regionBuckets)
{
    private readonly RegionBlockEntry[] buffer = new RegionBlockEntry[SectionVolume];
    private readonly byte[] compressed = new byte[SectionVolume * RegionBlockEntry.Size];

    public void Read(Vector3i sloc)
    {
        var cloc = sloc.Xy;
        var rloc = cloc.ToRloc();
        var index = regionIndexLoader.EnsureLoaded(rloc);
        var files = regions.Get(rloc).RegionFiles();

        var origin = new Vector3i(rloc.X << RegionBits, rloc.Y << RegionBits, 0);
        var offset = sloc - origin;
        var alloc = index[offset];

        var mem = blocksRaw.Span(cloc);
        var blocks = mem.Slice(sloc.Z * SectionVolume, SectionVolume);

        RandomAccess.Read(regionFileHandles[files.Buckets[alloc.Bucket]],
            compressed.AsSpan()[..alloc.Count],
            alloc.Offset * regionBuckets.Sizes[alloc.Bucket]);

        BrotliDecoder.TryDecompress(compressed.AsSpan()[..alloc.Count], MemoryMarshal.AsBytes(buffer.AsSpan()), out var bytes);
        int count = bytes / RegionBlockEntry.Size;

        var entries = buffer.AsSpan()[..count];
        int cur = 0;

        foreach (var entry in entries)
        {
            var block = moduleIndices[entry.Value];

            for (int i = 0; i < entry.Count; i++)
                blocks[cur++] = block;
        }
    }
}
