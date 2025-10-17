namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRegionWriter(
    WorldModuleIndices moduleIndices,
    DimensionBlocksRaw blocksRaw,
    DimensionRegions regions,
    DimensionRegionFileHandles regionFileHandles,
    DimensionRegionIndexLoader regionIndexLoader,
    DimensionRegionBuckets regionBuckets)
{
    private readonly RegionBlockEntry[] buffer = new RegionBlockEntry[SectionVolume];
    private readonly byte[] compressed = new byte[SectionVolume * RegionBlockEntry.Size];
    private readonly byte[] zeroes = new byte[SectionVolume * RegionBlockEntry.Size];
    private int bytes;

    public void Write(Vector3i sloc)
    {
        var cloc = sloc.Xy;
        var rloc = cloc.ToRloc();
        var index = regionIndexLoader.EnsureLoaded(rloc);
        var files = regions.Get(rloc).RegionFiles();
        var freeMap = regions.Get(rloc).RegionFreeMap();

        var origin = new Vector3i(rloc.X << RegionBits, rloc.Y << RegionBits, 0);
        var offset = sloc - origin;
        ref var alloc = ref index[offset];

        var mem = blocksRaw.Span(cloc);
        var blocks = mem.Slice(sloc.Z * SectionVolume, SectionVolume);

        EncodeIntoBuffer(blocks);

        RandomAccess.Write(regionFileHandles[files.Buckets[alloc.Bucket]],
            zeroes.AsSpan()[..alloc.Count],
            alloc.Offset * regionBuckets.Sizes[alloc.Bucket]);

        if (regionBuckets.Sizes[alloc.Bucket] <= bytes)
        {
            if (alloc.Bucket != 0)
                freeMap.Free(alloc.Bucket, alloc.Offset);

            alloc.Bucket = (byte)regionBuckets.BestFit(bytes);
            alloc.Offset = (ushort)freeMap.Alloc(alloc.Bucket);
        }

        alloc.Count = (ushort)bytes;

        var findex = regionFileHandles[files.Index];
        RandomAccess.Write(findex, MemoryMarshal.AsBytes(index.Span.Slice(index.Index(offset), 1)),
            index.Index(offset) * RegionIndexEntry.Size);

        var bucket = regionFileHandles[files.Buckets[alloc.Bucket]];
        RandomAccess.Write(bucket, compressed.AsSpan()[..bytes],
            alloc.Offset * regionBuckets.Sizes[alloc.Bucket]);
    }

    private void EncodeIntoBuffer(ReadOnlySpan<Ent> blocks)
    {
        int count = 0;
        Ent prev = default;
        int run = 0;

        foreach (var block in blocks)
        {
            if (block != prev)
            {
                Flush();
                prev = block;
                run = 1;
            }
            else run++;
        }

        Flush();

        BrotliEncoder.TryCompress(MemoryMarshal.AsBytes(buffer.AsSpan()[..count]), compressed, out bytes);

        void Flush()
        {
            if (run > 0)
                buffer[count++] = new() { Value = moduleIndices[prev], Count = run };
        }
    }
}
