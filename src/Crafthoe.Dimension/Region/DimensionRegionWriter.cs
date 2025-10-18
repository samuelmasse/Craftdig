namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRegionWriter(
    WorldModuleIndices moduleIndices,
    DimensionBlocksRaw blocksRaw,
    DimensionRegionFileHandles regionFileHandles,
    DimensionRegionStates regionStates,
    DimensionRegionBuckets regionBuckets)
{
    private readonly RegionBlockEntry[] buffer = new RegionBlockEntry[SectionVolume];
    private readonly byte[] compressed = new byte[SectionVolume * RegionBlockEntry.Size];
    private readonly byte[] zeroes = new byte[SectionVolume * RegionBlockEntry.Size];
    private int bytes;

    public void Write(Vector3i sloc)
    {
        var state = regionStates[sloc.Xy.ToRloc()];
        var offset = sloc - state.Origin;
        ref var alloc = ref state.Index[offset];

        EncodeIntoBuffer(blocksRaw.Slice(sloc));

        RandomAccess.Write(regionFileHandles[state.Files.Buckets[alloc.Bucket]],
            zeroes.AsSpan()[..alloc.Count],
            alloc.Offset * regionBuckets.Sizes[alloc.Bucket]);

        if (regionBuckets.Sizes[alloc.Bucket] <= bytes)
        {
            if (alloc.Bucket != 0)
                state.FreeMap.Free(alloc.Bucket, alloc.Offset);

            alloc.Bucket = (byte)regionBuckets.BestFit(bytes);
            alloc.Offset = (ushort)state.FreeMap.Alloc(alloc.Bucket);
        }

        alloc.Count = (ushort)bytes;

        var findex = regionFileHandles[state.Files.Index];
        RandomAccess.Write(findex, MemoryMarshal.AsBytes(state.Index.Span.Slice(state.Index.Index(offset), 1)),
            state.Index.Index(offset) * RegionIndexEntry.Size);

        var bucket = regionFileHandles[state.Files.Buckets[alloc.Bucket]];
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
