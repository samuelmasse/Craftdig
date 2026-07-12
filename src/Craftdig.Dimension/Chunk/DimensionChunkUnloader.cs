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
        chunk.ChunkLight?.Clear();
        chunk.ChunkLight = null;
        chunks.Free(cloc);
    }
}
