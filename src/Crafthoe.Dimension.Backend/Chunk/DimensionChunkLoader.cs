namespace Crafthoe.Dimension.Backend;

[Dimension]
public class DimensionChunkLoader(
    DimensionRegionThreadWorkQueue regionThreadWorkQueue,
    DimensionChunkPending chunkPending,
    DimensionBlocksPool blocksPool)
{
    public void Load(Vector2i cloc)
    {
        var blocks = blocksPool.Take();
        chunkPending.Add(cloc);
        regionThreadWorkQueue.Enqeue(new(new(cloc, 0), RegionThreadInputType.ReadChunk, blocks));
    }
}
