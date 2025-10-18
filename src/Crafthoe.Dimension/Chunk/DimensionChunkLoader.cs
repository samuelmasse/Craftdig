namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkLoader(
    DimensionChunkThreadWorkQueue chunkThreadWorkQueue,
    DimensionChunkPending chunkPending,
    DimensionRegionChunkReader chunkReader,
    DimensionBlocksPool blocksPool)
{
    public void Load(Vector2i cloc)
    {
        var blocks = blocksPool.Take();
        bool noop = chunkReader.TryRead(blocks.Span, cloc);

        chunkPending.Add(cloc);
        chunkThreadWorkQueue.Enqeue(new(blocks, cloc, noop));
    }
}
