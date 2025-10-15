namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkLoader(
    DimensionChunks chunks,
    DimensionChunkReader chunkReader,
    DimensionTerrainGenerator chunkGenerator,
    DimensionBiomeGenerator biomeGenerator,
    DimensionMetrics metrics,
    DimensionChunkRenderScheduler chunkRenderScheduler,
    DimensionChunkBag chunkIndex,
    DimensionRegionWriter regionWriter)
{
    public void Load(Vector2i cloc)
    {
        var chunk = chunks.Get(cloc);

        if (!chunkReader.TryRead(cloc))
        {
            metrics.ChunkMetric.Start();
            chunkGenerator.Generate(cloc);
            biomeGenerator.Generate(cloc);

            for (int sz = 0; sz < SectionHeight; sz++)
                regionWriter.Write(new(cloc, sz));

            metrics.ChunkMetric.End();
        }

        chunkRenderScheduler.Add(cloc);
        chunkIndex.Add(chunk);
    }
}
