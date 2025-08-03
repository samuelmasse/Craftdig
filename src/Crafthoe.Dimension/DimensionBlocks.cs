namespace Crafthoe.Dimension;

[Dimension]
public class DimensionBlocks(DimensionChunks chunks)
{
    private readonly Queue<Memory<Ent>> pool = [];

    public bool TryGet(Vector3i loc, out Ent block)
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

    public bool TrySet(Vector3i loc, Ent block)
    {
        if ((uint)loc.Z >= HeightSize)
            return false;

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

    public Span<Ent> ChunkBlocks(Vector2i cloc)
    {
        if (!chunks.TryGet(cloc, out var chunk))
            return default;

        ref var blocks = ref chunk.Blocks();
        if (blocks.Length == 0)
            blocks = pool.Count > 0 ? pool.Dequeue() : new Ent[HeightSize * SectionSize * SectionSize];

        return blocks.Span;
    }

    public void ReturnBlocks(Memory<Ent> blocks)
    {
        blocks.Span.Clear();
        pool.Enqueue(blocks);
    }
}
