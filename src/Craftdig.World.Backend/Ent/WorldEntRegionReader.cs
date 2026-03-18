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
        long index = 0;
        var span = buffer.GetSpan(unit)[..unit];

        while (index < length)
        {
            RandomAccess.Read(handle, span, index);

            var id = MemoryMarshal.Read<Guid>(span);
            if (id == default)
            {
                index += unit;
                continue;
            }

            int size = Unsafe.SizeOf<Guid>();
            var ent = entArena.Alloc().Mutate()
                .Id(id)
                .IsLoading(true)
                .Ent;

            while (size < unit)
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

            ent.Ploc = new(region.Rloc, (byte)bucket, (int)(index / unit), size);
            ent.IsLoading = false;
            index += unit;

            region.FreeMap.Take(bucket, (int)(index / unit));
            region.Ents.Add(ent);
        }
    }
}
