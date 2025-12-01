namespace Crafthoe.Dimension.Frontend;

[Dimension]
public class DimensionChunkRenderScheduler(DimensionChunks chunks, DimensionBlocksRaw blocksRaw)
{
    public void Add(Vector2i cloc)
    {
        Process(cloc);
        Process(cloc + (1, 0));
        Process(cloc + (0, 1));
        Process(cloc + (-1, 0));
        Process(cloc + (0, -1));
        Process(cloc + (1, 1));
        Process(cloc + (-1, 1));
        Process(cloc + (-1, -1));
        Process(cloc + (1, -1));
    }

    private void Process(Vector2i cloc)
    {
        if (IsNull(cloc + (1, 0)) ||
            IsNull(cloc + (0, 1)) ||
            IsNull(cloc + (-1, 0)) ||
            IsNull(cloc + (0, -1)) ||
            IsNull(cloc + (1, 1)) ||
            IsNull(cloc + (-1, 1)) ||
            IsNull(cloc + (-1, -1)) ||
            IsNull(cloc + (1, -1)))
            return;

        if (!chunks.TryGet(cloc, out var chunk))
            return;

        if (!blocksRaw.TryGetChunkBlocks(cloc, out var blocks))
            return;

        if (!chunk.IsUnrenderedListBuilt())
        {
            for (int sz = 0; sz < SectionHeight; sz++)
            {
                if (blocks.Uniform(sz) == default || blocks.Uniform(sz).IsSolid())
                    chunk.Unrendered().Add(sz, sz);
            }

            chunk.IsUnrenderedListBuilt() = true;
        }

        chunk.IsReadyToRender() = true;
    }

    private bool IsNull(Vector2i cloc) => !chunks.TryGet(cloc, out _);
}
