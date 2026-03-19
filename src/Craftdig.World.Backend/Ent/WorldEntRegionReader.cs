namespace Craftdig.World.Backend;

[World]
public class WorldEntRegionReader(
    WorldEntArena entArena,
    WorldComponentWriters componentWriters,
    WorldEntRegionBuckets entRegionBuckets,
    WorldEntRegionFileHandles entRegionFileHandles)
{
    private readonly ArrayBufferWriter<byte> buffer = new();

    public void ReadEntsFromRegion(EntRegionState region)
    {
        for (int i = 0; i < region.Files.Buckets.Length; i++)
        {
            var bucketFile = region.Files.Buckets[i];
            if (!File.Exists(bucketFile))
                continue;

            LoadEntsFromBucket(region, i);
        }
    }

    private void LoadEntsFromBucket(EntRegionState region, int bucket)
    {
        var handle = entRegionFileHandles[region.Files.Buckets[bucket]];
        int unit = entRegionBuckets.Sizes[bucket];
        long length = RandomAccess.GetLength(handle);

        var span = buffer.GetSpan(Math.Max(1024 * 1024, unit));
        long fileOffset = 0;

        while (fileOffset < length)
        {
            int read = (int)Math.Min(span.Length, length - fileOffset);
            RandomAccess.Read(handle, span, fileOffset);
            ProcessChunk(region, bucket, unit, fileOffset, span[..read]);
            fileOffset += read;
        }
    }

    private void ProcessChunk(EntRegionState region, int bucket, int unit, long fileOffset, Span<byte> data)
    {
        int count = data.Length / unit;
        int baseSlot = (int)(fileOffset / unit);

        for (int i = 0; i < count; i++)
        {
            int slot = baseSlot + i;
            var span = data.Slice(i * unit, unit);

            if (!TryReadEnt(span, out var ent, out int size))
                continue;

            ent.Ploc = new(region.Rloc, (byte)bucket, slot, size);
            ent.IsLoading = false;

            region.FreeMap.Take(bucket, slot);
            region.Ents.Add(ent);
        }
    }

    private bool TryReadEnt(ReadOnlySpan<byte> span, out EntMutIdx ent, out int size)
    {
        var id = MemoryMarshal.Read<Guid>(span);
        if (id == default)
        {
            ent = default;
            size = 0;
            return false;
        }

        ent = entArena.Alloc().Mutate()
            .Id(id)
            .IsLoading(true)
            .Ent;

        size = Unsafe.SizeOf<Guid>();
        ReadComponents(ent, span, ref size);
        return true;
    }

    private void ReadComponents(EntMutIdx ent, ReadOnlySpan<byte> span, ref int size)
    {
        while (size < span.Length)
        {
            int cindex = MemoryMarshal.Read<int>(span[size..]);
            if (cindex == 0)
                break;

            size += sizeof(int);
            int csize = MemoryMarshal.Read<int>(span[size..]);
            size += sizeof(int);

            var writer = componentWriters[cindex];
            writer.Read(ent, span.Slice(size, csize));
            writer.WritePloc(ent, new(size, csize));

            size += csize;
        }
    }
}
