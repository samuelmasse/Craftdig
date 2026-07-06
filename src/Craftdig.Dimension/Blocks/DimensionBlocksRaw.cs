namespace Craftdig.Dimension;

[Dimension]
public class DimensionBlocksRaw(DimensionChunks chunks)
{
    public bool TryGet(Vec3i loc, out Ent block)
    {
        if ((uint)loc.Z >= HeightSize)
        {
            block = default;
            return false;
        }

        var cloc = loc.XY.ToCloc();
        if (!TryGetChunkBlocks(cloc, out var blocks))
        {
            block = default;
            return false;
        }

        block = blocks[loc];
        return true;
    }

    public bool TrySet(Vec3i loc, Ent block)
    {
        if ((uint)loc.Z >= HeightSize)
            return false;

        var cloc = loc.XY.ToCloc();
        if (!TryGetChunkBlocks(cloc, out var blocks))
            return false;

        blocks[loc] = block;
        return true;
    }

    public bool TryGetChunkBlocks(Vec2i cloc, [NotNullWhen(true)] out ChunkBlocks? blocks)
    {
        chunks.TryGet(cloc, out var chunk);
        blocks = chunk.ChunkBlocks;
        return blocks != null;
    }
}
