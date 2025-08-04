namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkRenderScheduler(DimensionChunks chunks)
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

        if (!chunk.IsUnrenderedListBuilt())
        {
            var mem = chunk.GetBlocks();
            for (int sz = 0; sz < HeightSize / SectionSize; sz++)
            {
                int start = sz * SectionSize * SectionSize * SectionSize;
                var block = mem[start];
                bool uniform = true;

                for (int i = 1; i < SectionSize * SectionSize * SectionSize; i++)
                {
                    if (mem[start + i] != block)
                    {
                        uniform = false;
                        break;
                    }
                }

                if (!uniform || block.IsSolid())
                    chunk.Unrendered().Add(sz, sz);
            }

            chunk.IsUnrenderedListBuilt() = true;
        }

        chunk.IsReadyToRender() = true;
    }

    private bool IsNull(Vector2i cloc) => !chunks.TryGet(cloc, out _);
}
