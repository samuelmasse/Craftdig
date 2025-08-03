namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkLoader(
    DimensionChunks chunks,
    DimensionChunkGenerator chunkGenerator,
    DimensionMetrics metrics,
    DimensionChunkIndex chunkIndex,
    DimensionChunkGeneratedEvent chunkGeneratedEvent)
{
    public void Load(Vector2i cloc)
    {
        metrics.ChunkMetric.Start();

        var chunk = chunks.Alloc(cloc);
        chunkGenerator.Generate(cloc);
        chunkIndex.Add(chunk);
        chunkGeneratedEvent.Add(cloc);

        metrics.ChunkMetric.End();
    }
}
