namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRegionThreadReader(
    WorldModuleIndices moduleIndices,
    DimensionRegionThreadFileHandles fileHandles,
    DimensionRegionThreadStates states,
    DimensionRegionThreadBuckets buckets)
{
    private readonly RegionBlockEntry[] buffer = new RegionBlockEntry[SectionVolume];
    private readonly byte[] compressed = new byte[SectionVolume * RegionBlockEntry.Size];

    public void Read(Span<Ent> blocks, Vector3i sloc)
    {
        var state = states[sloc.Xy.ToRloc()];
        var alloc = state.Index[sloc - state.Origin];

        RandomAccess.Read(fileHandles[state.Files.Buckets[alloc.Bucket]],
            compressed.AsSpan()[..alloc.Count],
            alloc.Offset * buckets.Sizes[alloc.Bucket]);

        BrotliDecoder.TryDecompress(
            compressed.AsSpan()[..alloc.Count],
            MemoryMarshal.AsBytes(buffer.AsSpan()),
            out var bytes);

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
