namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkReader(
    DimensionRegionIndexLoader regionIndexLoader)
{
    public bool TryRead(Vector2i cloc)
    {
        var rloc = cloc.ToRloc();
        var index = regionIndexLoader.EnsureLoaded(rloc);

        var origin = new Vector2i(rloc.X << RegionBits, rloc.Y << RegionBits);
        var offset = cloc - origin;

        if (IsFirstSectionBlank(index, offset))
            return false;

        for (int sz = 0; sz < SectionHeight; sz++)
        {
            var alloc = index[new(offset, sz)];
            if (alloc.Bucket == 0)
                continue;


        }

        return false;
    }

    private bool IsFirstSectionBlank(RegionIndex index, Vector2i offset) =>
        index[new(offset, 0)].Bucket == 0;
}
