namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkLoader(
    DimensionChunks chunks,
    DimensionChunkGenerator chunkGenerator,
    DimensionMetrics metrics,
    DimensionChunkRenderScheduler chunkRenderScheduler,
    DimensionChunkIndex chunkIndex)
{
    public void Load(Vector2i cloc)
    {
        metrics.ChunkMetric.Start();

        var chunk = chunks.Alloc(cloc);
        chunkGenerator.Generate(cloc);
        chunkRenderScheduler.Add(cloc);
        chunkIndex.Add(chunk);

        metrics.ChunkMetric.End();
    }
}
