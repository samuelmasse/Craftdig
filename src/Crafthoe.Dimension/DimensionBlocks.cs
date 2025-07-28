namespace Crafthoe.Dimension;

[Dimension]
public class DimensionBlocks(DimensionChunks chunks)
{
    public bool TryGet(Vector3i loc, out ReadOnlyEntity block)
    {
        if (loc.Z < 0 || loc.Z >= chunks.Unit.Z)
        {
            block = default;
            return false;
        }    

        var cloc = new Vector2i(loc.X >= 0 ? loc.X / chunks.Unit.X : (loc.X - chunks.Unit.X + 1) / chunks.Unit.X,
            loc.Y >= 0 ? loc.Y / chunks.Unit.Y : (loc.Y - chunks.Unit.Y + 1) / chunks.Unit.Y);

        var blocks = ChunkBlocks(cloc);
        if (blocks.IsEmpty)
        {
            block = default;
            return false;
        }

        var dloc = loc - new Vector3i(cloc.X, cloc.Y, 0) * chunks.Unit;
        block = blocks[dloc.Z * chunks.Unit.X * chunks.Unit.Y + dloc.Y * chunks.Unit.X + dloc.X];
        return true;
    }

    public bool TrySet(Vector3i loc, ReadOnlyEntity block)
    {
        if (loc.Z < 0 || loc.Z >= chunks.Unit.Z)
            return false;

        var cloc = new Vector2i(loc.X >= 0 ? loc.X / chunks.Unit.X : (loc.X - chunks.Unit.X + 1) / chunks.Unit.X,
            loc.Y >= 0 ? loc.Y / chunks.Unit.Y : (loc.Y - chunks.Unit.Y + 1) / chunks.Unit.Y);

        var blocks = ChunkBlocks(cloc);
        if (blocks.IsEmpty)
            return false;

        var dloc = loc - new Vector3i(cloc.X, cloc.Y, 0) * chunks.Unit;
        blocks[dloc.Z * chunks.Unit.X * chunks.Unit.Y + dloc.Y * chunks.Unit.X + dloc.X] = block;
        return true;
    }

    private Span<ReadOnlyEntity> ChunkBlocks(Vector2i cloc)
    {
        var chunk = chunks[cloc];
        if (chunk == null)
            return default;

        var blocks = chunk.Value.ChunkBlocks();
        if (blocks.Length == 0)
        {
            chunk.Value.ChunkBlocks(new ReadOnlyEntity[chunks.Unit.X * chunks.Unit.Y * chunks.Unit.Z]);
            return chunk.Value.ChunkBlocks();
        }

        return blocks;
    }
}
