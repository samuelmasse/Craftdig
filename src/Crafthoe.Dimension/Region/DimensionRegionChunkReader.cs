namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRegionChunkReader(
    DimensionRegionStates regionStates,
    DimensionRegionReader regionReader)
{
    public bool TryRead(Vector2i cloc)
    {
        var state = regionStates[cloc.ToRloc()];
        var offset = cloc - state.Origin.Xy;

        if (IsFirstSectionBlank(state.Index, offset))
            return false;

        for (int sz = 0; sz < SectionHeight; sz++)
            regionReader.Read(new(cloc, sz));

        return true;
    }

    private bool IsFirstSectionBlank(RegionIndex index, Vector2i offset) =>
        index[new(offset, 0)].Bucket == 0;
}
