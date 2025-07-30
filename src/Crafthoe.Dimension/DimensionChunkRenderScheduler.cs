namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkRenderScheduler(DimensionChunks chunks, DimensionChunkGeneratedEvent chunkGeneratedEvent)
{
    public void Tick()
    {
        foreach (var e in chunkGeneratedEvent.Events)
        {
            Process(e);
            Process(e + (1, 0));
            Process(e + (0, 1));
            Process(e + (-1, 0));
            Process(e + (0, -1));
            Process(e + (1, 1));
            Process(e + (-1, 1));
            Process(e + (-1, -1));
            Process(e + (1, -1));
        }
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

        var chunk = chunks[cloc].GetValueOrDefault();
        var unrendered = chunk.ChunkUnrendered();
        if (unrendered != null)
            return;

        unrendered = [];
        chunk.ChunkUnrendered(unrendered);

        var mem = chunk.ChunkBlocks();
        for (int sz = 0; sz < HeightSize / SectionSize; sz++)
        {
            int start = sz * SectionSize * SectionSize * SectionSize;
            var block = mem[start];
            bool uniform = true;

            for (int i = 1; i < SectionSize * SectionSize; i++)
            {
                if (mem[start + i] != block)
                {
                    uniform = false;
                    break;
                }
            }

            if (!uniform || block.IsSolid())
                unrendered.Add(sz);
        }
    }

    private bool IsNull(Vector2i cloc) => chunks[cloc] == null;
}
