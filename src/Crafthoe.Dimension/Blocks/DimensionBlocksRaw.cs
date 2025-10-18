namespace Crafthoe.Dimension;

[Dimension]
public class DimensionBlocksRaw(DimensionChunks chunks)
{
    private readonly Queue<Memory<Ent>> pool = [];

    public bool TryGet(Vector3i loc, out Ent block)
    {
        if ((uint)loc.Z >= HeightSize)
        {
            block = default;
            return false;
        }

        var cloc = loc.Xy.ToCloc();
        var blocks = Span(cloc);
        if (blocks.IsEmpty)
        {
            block = default;
            return false;
        }

        block = blocks[loc.ToInnerIndex()];
        return true;
    }

    public bool TrySet(Vector3i loc, Ent block)
    {
        if ((uint)loc.Z >= HeightSize)
            return false;

        var cloc = loc.Xy.ToCloc();
        var blocks = Span(cloc);
        if (blocks.IsEmpty)
            return false;

        blocks[loc.ToInnerIndex()] = block;
        return true;
    }

    public Span<Ent> Span(Vector2i cloc)
    {
        if (!chunks.TryGet(cloc, out var chunk))
            return default;

        ref var blocks = ref chunk.Blocks();
        if (blocks.Length == 0)
            blocks = pool.Count > 0 ? pool.Dequeue() : new Ent[ChunkVolume];

        return blocks.Span;
    }

    public Span<Ent> Slice(Vector3i sloc) => Span(sloc.Xy).Slice(sloc.Z * SectionVolume, SectionVolume);

    public void Return(Memory<Ent> blocks)
    {
        blocks.Span.Clear();
        pool.Enqueue(blocks);
    }
}
