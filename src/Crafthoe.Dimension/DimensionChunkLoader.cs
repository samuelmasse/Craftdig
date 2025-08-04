namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkLoader(
    DimensionChunks chunks,
    DimensionTerrainGenerator chunkGenerator,
    DimensionBiomeGenerator biomeGenerator,
    DimensionMetrics metrics,
    DimensionChunkRenderScheduler chunkRenderScheduler,
    DimensionChunkIndex chunkIndex)
{
    public void Load(Vector2i cloc)
    {
        metrics.ChunkMetric.Start();

        var chunk = chunks.Alloc(cloc);
        chunkGenerator.Generate(cloc);
        biomeGenerator.Generate(cloc);
        chunkRenderScheduler.Add(cloc);
        chunkIndex.Add(chunk);

        metrics.ChunkMetric.End();
    }
}
