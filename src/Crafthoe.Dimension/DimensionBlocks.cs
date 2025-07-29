namespace Crafthoe.Dimension;

[Dimension]
public class DimensionBlocks(DimensionChunks chunks)
{
    public bool TryGet(Vector3i loc, out ReadOnlyEntity block)
    {
        if ((uint)loc.Z >= HeightSize)
        {
            block = default;
            return false;
        }

        int cx = loc.X >> SectionBits;
        int cy = loc.Y >> SectionBits;

        var blocks = ChunkBlocks((cx, cy));
        if (blocks.IsEmpty)
        {
            block = default;
            return false;
        }

        int lx = loc.X & SectionMask;
        int ly = loc.Y & SectionMask;
        int lz = loc.Z;
        int index = (lz << (SectionBits * 2)) + (ly << SectionBits) + lx;

        block = blocks[index];
        return true;
    }

    public bool TrySet(Vector3i loc, ReadOnlyEntity block)
    {
        if ((uint)loc.Z >= HeightSize)
            block = default;

        int cx = loc.X >> SectionBits;
        int cy = loc.Y >> SectionBits;

        var blocks = ChunkBlocks((cx, cy));
        if (blocks.IsEmpty)
            return false;

        int lx = loc.X & SectionMask;
        int ly = loc.Y & SectionMask;
        int lz = loc.Z;
        int index = (lz << (SectionBits * 2)) + (ly << SectionBits) + lx;

        blocks[index] = block;
        return true;
    }

    public Span<ReadOnlyEntity> ChunkBlocks(Vector2i cloc)
    {
        var chunk = chunks[cloc];
        if (chunk == null)
            return default;

        var blocks = chunk.Value.ChunkBlocks();
        if (blocks.Length == 0)
        {
            chunk.Value.ChunkBlocks(new ReadOnlyEntity[HeightSize * SectionSize * SectionSize]);
            return chunk.Value.ChunkBlocks();
        }

        return blocks;
    }
}
