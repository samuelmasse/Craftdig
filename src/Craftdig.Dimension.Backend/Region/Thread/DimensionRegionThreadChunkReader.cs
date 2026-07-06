namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionRegionThreadChunkReader(
    DimensionRegionThreadStates states,
    DimensionRegionThreadReader reader)
{
    public bool TryRead(ChunkBlocks blocks, Vec2i cloc)
    {
        var state = states[cloc.ToRloc()];
        var offset = cloc - state.Origin.XY;

        if (IsFirstSectionBlank(state.Index, offset))
            return false;

        for (int sz = 0; sz < SectionHeight; sz++)
            reader.Read(blocks, sz, new(cloc, sz));

        return true;
    }

    private bool IsFirstSectionBlank(RegionIndex index, Vec2i offset) =>
        index[new(offset, 0)].Bucket == 0;
}
