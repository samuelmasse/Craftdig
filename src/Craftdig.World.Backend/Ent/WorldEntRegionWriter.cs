namespace Craftdig.World.Backend;

[World]
public class WorldEntRegionWriter(
    WorldIndexedComponents indexedComponents,
    WorldComponentWriters componentWriters,
    WorldEntRegionBuckets entRegionBuckets,
    WorldEntRegionFileHandles entRegionFileHandles,
    WorldEntRegionStates entRegionStates)
{
    private readonly ArrayBufferWriter<byte> buffer = new();

    public void Write(EntMutIdx ent)
    {
        var rloc = Rloc(ent);
        var ploc = ent.Ploc;

        if (ploc == null)
            AddToRegion(ent, rloc);
        else if (ploc.Value.Rloc == rloc)
            UpdateInRegion(ent, rloc, ploc.Value);
        else MoveToRegion(ent, rloc);
    }

    public void Erase(EntMutIdx ent)
    {
        RemoveFromRegion(ent);
    }

    private void AddToRegion(EntMutIdx ent, Vec2i rloc)
    {
        var region = entRegionStates[rloc];
        int size = Unsafe.SizeOf<Guid>();

        foreach (var index in indexedComponents.Indices)
        {
            var writer = componentWriters[index];
            if (writer.Has(ent))
            {
                size += sizeof(int);
                size += sizeof(int);
                size += writer.Measure(ent);
            }
        }

        var bucket = entRegionBuckets.BestFit(size);
        int bucketSize = entRegionBuckets.Sizes[bucket];
        int offset = region.FreeMap.Alloc(bucket);
        region.Ents.Add(ent);

        var span = buffer.GetSpan(bucketSize)[..bucketSize];
        span.Clear();

        int spIndex = 0;

        MemoryMarshal.Write(span, ent.Id);
        spIndex += Unsafe.SizeOf<Guid>();

        foreach (var index in indexedComponents.Indices)
        {
            var writer = componentWriters[index];
            if (!writer.Has(ent))
                continue;

            MemoryMarshal.Write(span[spIndex..], index);
            spIndex += sizeof(int);

            int csize = writer.Measure(ent);
            MemoryMarshal.Write(span[spIndex..], csize);
            spIndex += sizeof(int);

            writer.Write(ent, span[spIndex..]);

            writer.WritePloc(ent, new(spIndex, csize));
            spIndex += csize;
        }

        var handle = entRegionFileHandles[region.Files.Buckets[bucket]];

        RandomAccess.Write(handle, span, offset * entRegionBuckets.Sizes[bucket]);

        ent.Ploc = new(rloc, (byte)bucket, offset, size);
    }

    private void UpdateInRegion(EntMutIdx ent, Vec2i rloc, EntPloc ploc)
    {
        var dirty = ent.DirtyComponents;
        if (dirty == null)
            return;

        if (!TryUpdateComponentsInPlace(ent, rloc, ploc, dirty))
            MoveToRegion(ent, rloc);
    }

    private bool TryUpdateComponentsInPlace(EntMutIdx ent, Vec2i rloc, EntPloc ploc, ulong[] dirty)
    {
        for (int i = 0; i < dirty.Length; i++)
        {
            ulong bits = dirty[i];
            int offset = i * 64;

            while (bits != 0)
            {
                int bit = System.Numerics.BitOperations.TrailingZeroCount(bits);
                int index = offset + bit;
                bits &= bits - 1;

                if (!TryUpdateComponentInPlace(ent, rloc, ref ploc, index))
                    return false;
            }
        }

        return true;
    }

    private bool TryUpdateComponentInPlace(EntMutIdx ent, Vec2i rloc, ref EntPloc ploc, int index)
    {
        var region = entRegionStates[rloc];
        var handle = entRegionFileHandles[region.Files.Buckets[ploc.Bucket]];
        int bucketSize = entRegionBuckets.Sizes[ploc.Bucket];

        var writer = componentWriters[index];
        int free = bucketSize - ploc.Size;
        var cploc = writer.ReadPloc(ent);

        if (cploc == null)
        {
            if (!writer.Has(ent))
                return true; // can happen if the component was set and unset within the same tick

            int size = writer.Measure(ent);
            int totalSize = size + sizeof(int) + sizeof(int);

            if (free >= totalSize)
            {
                var span = buffer.GetSpan(totalSize)[..totalSize];

                MemoryMarshal.Write(span, index);
                MemoryMarshal.Write(span[sizeof(int)..], size);
                writer.Write(ent, span[(sizeof(int) + sizeof(int))..]);

                RandomAccess.Write(handle, span, ploc.Index * bucketSize + ploc.Size);

                writer.WritePloc(ent, new(ploc.Size + sizeof(int) + sizeof(int), size));
                ploc = ploc with { Size = ploc.Size + totalSize };
                ent.Ploc = ploc;
            }
            else return false; // cannot add new component in place due to insufficient free space
        }
        else
        {
            int size = writer.Measure(ent);
            if (size == cploc.Value.Size)
            {
                var span = buffer.GetSpan(size)[..size];
                writer.Write(ent, span);

                RandomAccess.Write(handle, span, ploc.Index * bucketSize + cploc.Value.Offset);
            }
            else return false; // cannot write in place because the component size changed
        }

        return true;
    }

    private void RemoveFromRegion(EntMutIdx ent)
    {
        if (ent.Ploc == null)
            return;

        var ploc = ent.Ploc.GetValueOrDefault();
        var region = entRegionStates[ploc.Rloc];
        var handle = entRegionFileHandles[region.Files.Buckets[ploc.Bucket]];

        var span = buffer.GetSpan(ploc.Size)[..ploc.Size];
        span.Clear();
        RandomAccess.Write(handle, span, ploc.Index * entRegionBuckets.Sizes[ploc.Bucket]);

        region.FreeMap.Free(ploc.Bucket, ploc.Index);
        region.Ents.Remove(ent);
        ent.Ploc = null;
    }

    private void MoveToRegion(EntMutIdx ent, Vec2i next)
    {
        RemoveFromRegion(ent);
        AddToRegion(ent, next);
    }

    protected virtual Vec2i Rloc(EntMutIdx ent) => default;
}
