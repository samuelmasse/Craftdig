namespace Crafthoe.Dimension.Backend;

[Dimension]
public class DimensionChunkLoader(
    DimensionRegionThreadWorkQueue regionThreadWorkQueue,
    DimensionChunkPending chunkPending)
{
    public void Load(Vector2i cloc)
    {
        chunkPending.Add(cloc);
        regionThreadWorkQueue.Enqeue(new(new(cloc, 0), RegionThreadInputType.ReadChunk, new ChunkBlocks(), default));
    }
}
