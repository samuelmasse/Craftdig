namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkUnloader(
    DimensionChunks chunks,
    DimensionChunkBag chunkBag,
    DimensionChunkUnloaderHandlers chunkUnloaderHandlers)
{
    public void Unload(Vector2i cloc)
    {
        if (!chunks.TryGet(cloc, out var chunk))
            return;

        chunkUnloaderHandlers.Run(chunk);
        chunkBag.Remove(chunk);
        chunks.Free(cloc);
    }
}
