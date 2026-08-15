namespace Craftdig;

[Dimension]
public class DimensionChunkLoader(
    DimensionRegionThreadWorkQueue regionThreadWorkQueue,
    DimensionChunkPending chunkPending,
    DimensionBlocksAllocator blocksAllocator)
{
    public void Load(Vec2i cloc)
    {
        chunkPending.Add(cloc);
        regionThreadWorkQueue.Enqeue(new(new(cloc, 0), RegionThreadInputType.ReadChunk, new(blocksAllocator), default));
    }
}
