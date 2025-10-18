namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRegionStates(
    DimensionPaths paths,
    DimensionRegionBuckets regionBuckets,
    DimensionRegionFileHandles regionFileHandles,
    DimensionRegions regions)
{
    public RegionState this[Vector2i rloc]
    {
        get
        {
            if (!regions.Contains(rloc))
                regions.Alloc(rloc);

            ref var state = ref regions[rloc].RegionState();
            state ??= InitializeState(rloc);

            return state;
        }
    }

    private RegionState InitializeState(Vector2i rloc)
    {
        var state = new RegionState(paths.Regions, rloc, regionBuckets.Count);
        var handle = regionFileHandles[state.Files.Index];

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
