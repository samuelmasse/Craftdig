namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkReader(
    DimensionRegionIndexLoader regionIndexLoader,
    DimensionRegionReader regionReader)
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
            regionReader.Read(new(cloc, sz));

        return true;
    }

    private bool IsFirstSectionBlank(RegionIndex index, Vector2i offset) =>
        index[new(offset, 0)].Bucket == 0;
}
