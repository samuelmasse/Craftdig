namespace Craftdig;

[Dimension]
public class DimensionChunkRenderDescheduler(DimensionChunks chunks)
{
    public void Remove(Vec2i cloc)
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

    private void Process(Vec2i cloc)
    {
        if (!chunks.TryGet(cloc, out var chunk))
            return;

        chunk.IsReadyToRender = false;
    }
}
