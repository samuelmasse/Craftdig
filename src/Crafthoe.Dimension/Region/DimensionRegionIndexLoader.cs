namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRegionIndexLoader(
    DimensionPaths paths,
    DimensionRegionBuckets regionBuckets,
    DimensionRegionFileHandles regionFileHandles,
    DimensionRegions regions)
{
    public RegionIndex EnsureLoaded(Vector2i rloc)
    {
        var region = regions.Get(rloc);
        ref var index = ref region.RegionIndex();
        if (index != null)
            return index;

        index = new();

        ref var files = ref region.RegionFiles();
        files = new(paths.Regions, rloc);

        ref var freeMap = ref region.RegionFreeMap();
        freeMap = new(regionBuckets.Count);

        var handle = regionFileHandles[files.Index];

        if (RandomAccess.GetLength(handle) == 0)
            RandomAccess.Write(handle, index.Bytes, 0);
        else
        {
            RandomAccess.Read(handle, index.Bytes, 0);

            foreach (var alloc in index.Span)
            {
                if (alloc.Bucket != 0)
                    freeMap.Take(alloc.Bucket, alloc.Offset);
            }
        }

        return index;
    }
}
