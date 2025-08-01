namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkLoader(
    DimensionChunks chunks,
    DimensionChunkGenerator chunkGenerator,
    DimensionMetrics metrics,
    DimensionChunkGeneratedEvent chunkGeneratedEvent)
{
    public void Load(Vector2i cloc)
    {
        metrics.ChunkMetric.Start();

        chunks.Alloc(cloc);
        chunkGenerator.Generate(cloc);
        chunkGeneratedEvent.Add(cloc);

        metrics.ChunkMetric.End();
    }
}
