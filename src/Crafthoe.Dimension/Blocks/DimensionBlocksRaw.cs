namespace Crafthoe.Dimension;

[Dimension]
public class DimensionBlocksRaw(DimensionChunks chunks)
{
    public bool TryGet(Vector3i loc, out Ent block)
    {
        if ((uint)loc.Z >= HeightSize)
        {
            block = default;
            return false;
        }

        var cloc = loc.Xy.ToCloc();
        var blocks = Memory(cloc).Span;
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
        var blocks = Memory(cloc).Span;
        if (blocks.IsEmpty)
            return false;

        blocks[loc.ToInnerIndex()] = block;
        return true;
    }

    public Memory<Ent> Slice(Vector3i sloc) => Memory(sloc.Xy).Slice(sloc.Z * SectionVolume, SectionVolume);

    public Memory<Ent> Memory(Vector2i cloc)
    {
        if (!chunks.TryGet(cloc, out var chunk))
            return default;

        return chunk.Blocks();
    }
}
