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
        if (!TryGetChunkBlocks(cloc, out var blocks))
        {
            block = default;
            return false;
        }

        block = blocks[loc];
        return true;
    }

    public bool TrySet(Vector3i loc, Ent block)
    {
        if ((uint)loc.Z >= HeightSize)
            return false;

        var cloc = loc.Xy.ToCloc();
        if (!TryGetChunkBlocks(cloc, out var blocks))
            return false;

        blocks[loc] = block;
        return true;
    }

    public bool TryGetChunkBlocks(Vector2i cloc, [NotNullWhen(true)] out ChunkBlocks? blocks)
    {
        chunks.TryGet(cloc, out var chunk);
        blocks = chunk.GetChunkBlocks();

        if (blocks == null)
        {
            if (!chunk.IsAlive)
                return false;

            blocks = new();
            chunk.ChunkBlocks() = blocks;
        }

        return true;
    }

    public ChunkBlocks ChunkBlocks(Vector2i cloc)
    {
        TryGetChunkBlocks(cloc, out var blocks);
        if (blocks == null)
            throw new NullReferenceException();

        return blocks;
    }
}
