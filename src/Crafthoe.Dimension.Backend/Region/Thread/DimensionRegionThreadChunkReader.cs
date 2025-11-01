namespace Crafthoe.Dimension.Backend;

[Dimension]
public class DimensionRegionThreadChunkReader(
    DimensionRegionThreadStates states,
    DimensionRegionThreadReader reader)
{
    public bool TryRead(Span<Ent> blocks, Vector2i cloc)
    {
        var state = states[cloc.ToRloc()];
        var offset = cloc - state.Origin.Xy;

        if (IsFirstSectionBlank(state.Index, offset))
            return false;

        for (int sz = 0; sz < SectionHeight; sz++)
            reader.Read(blocks.Slice(sz * SectionVolume, SectionVolume), new(cloc, sz));

        return true;
    }

    private bool IsFirstSectionBlank(RegionIndex index, Vector2i offset) =>
        index[new(offset, 0)].Bucket == 0;
}
