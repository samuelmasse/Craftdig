namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionRegionThreadStates(
    DimensionPaths paths,
    DimensionRegionThreadBuckets buckets,
    DimensionRegionThreadFileHandles fileHandles)
{
    private readonly Dictionary<Vector2i, RegionState> dict = [];
    private readonly Dictionary<Vector2i, DateTime> access = [];
    private readonly List<Vector2i> remove = [];
    private readonly List<string> flush = [];

    public RegionState this[Vector2i rloc]
    {
        get
        {
            if (!dict.TryGetValue(rloc, out var state))
            {
                state = New(rloc);
                dict.Add(rloc, state);
            }

            access[rloc] = DateTime.UtcNow;

            return state;
        }
    }

    public void Drain()
    {
        if (dict.Count < 8)
            return;

        var now = DateTime.UtcNow;

        foreach (var (rloc, time) in access)
        {
            if ((now - time).TotalSeconds > 5)
                remove.Add(rloc);
        }

        foreach (var rloc in remove)
        {
            var state = dict[rloc];
            flush.Add(state.Files.Index);
            foreach (var item in state.Files.Buckets)
                flush.Add(item);

            access.Remove(rloc);
            dict.Remove(rloc);
        }

        remove.Clear();

        if (flush.Count == 0)
            return;

        fileHandles.Drain(CollectionsMarshal.AsSpan(flush));
        flush.Clear();
    }

    private RegionState New(Vector2i rloc)
    {
        var state = new RegionState(paths.Regions, rloc, buckets.Count);
        var handle = fileHandles[state.Files.Index];

        if (RandomAccess.GetLength(handle) != 0)
        {
            RandomAccess.Read(handle, state.Index.Bytes, 0);

            foreach (var alloc in state.Index.Span)
            {
                if (alloc.Bucket != 0)
                    state.FreeMap.Take(alloc.Bucket, alloc.Offset);
            }
        }
        else RandomAccess.Write(handle, state.Index.Bytes, 0);

        return state;
    }
}
