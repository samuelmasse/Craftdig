namespace Craftdig.Dimension;

[Dimension]
public class DimensionChunkUnloader(
    DimensionChunks chunks,
    DimensionChunkUnloaderHandlers chunkUnloaderHandlers)
{
    public void Unload(Vec2i cloc)
    {
        if (!chunks.TryGet(cloc, out var chunk))
            return;

        chunkUnloaderHandlers.Run(chunk);
        chunks.Free(cloc);
    }
}
