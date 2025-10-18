namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRegionThreadStates(
    DimensionPaths paths,
    DimensionRegionThreadBuckets buckets,
    DimensionRegionThreadFileHandles fileHandles)
{
    private readonly Dictionary<Vector2i, RegionState> dict = [];

    public RegionState this[Vector2i rloc]
    {
        get
        {
            if (dict.TryGetValue(rloc, out var state))
                return state;

            state = InitializeState(rloc);
            dict.Add(rloc, state);

            return state;
        }
    }

    private RegionState InitializeState(Vector2i rloc)
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
