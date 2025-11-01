namespace Crafthoe.Dimension.Backend;

[Dimension]
public class DimensionChunkUnloader(
    DimensionChunks chunks,
    DimensionChunkBag chunkIndex,
    DimensionRegionThreadWorkQueue regionThreadWorkQueue,
    DimensionRegionInvalidation regionInvalidation,
    DimensionChunkUnloaderHandlers chunkUnloaderHandlers)
{
    public void Unload(Vector2i cloc)
    {
        if (!chunks.TryGet(cloc, out var chunk))
            return;

        for (int sz = 0; sz < SectionHeight; sz++)
            regionInvalidation.Drain(new(cloc, sz));

        if (!chunk.Blocks().IsEmpty)
            regionThreadWorkQueue.Enqeue(new(new(cloc, 0), RegionThreadInputType.DisposeChunk, chunk.Blocks()));

        chunkIndex.Remove(chunk);
        chunkUnloaderHandlers.Run(chunk);

        chunks.Free(cloc);
    }
}
