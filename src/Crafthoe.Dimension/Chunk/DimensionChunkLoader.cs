namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkLoader(
    DimensionChunks chunks,
    DimensionRegionChunkReader chunkReader,
    DimensionTerrainGenerator chunkGenerator,
    DimensionBiomeGenerator biomeGenerator,
    DimensionMetrics metrics,
    DimensionChunkRenderScheduler chunkRenderScheduler,
    DimensionChunkBag chunkBag,
    DimensionRegionWriter regionWriter)
{
    public void Load(Vector2i cloc)
    {
        chunks.Alloc(cloc);
        var chunk = chunks[cloc];

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
        chunkBag.Add(chunk);
    }
}
