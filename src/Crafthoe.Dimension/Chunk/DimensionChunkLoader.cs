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
    DimensionBlocksPool blocksPool,
    DimensionRegionWriter regionWriter)
{
    public void Load(Vector2i cloc)
    {
        var blocks = blocksPool.Take();

        if (!chunkReader.TryRead(blocks.Span, cloc))
        {
            metrics.ChunkMetric.Start();

            chunkGenerator.Generate(blocks.Span, cloc);
            biomeGenerator.Generate(blocks.Span, cloc);

            for (int sz = 0; sz < SectionHeight; sz++)
                regionWriter.Write(blocks.Span.Slice(sz * SectionVolume, SectionVolume), new(cloc, sz));

            metrics.ChunkMetric.End();
        }

        chunks.Alloc(cloc);
        var chunk = chunks[cloc];
        chunk.Blocks(blocks);

        chunkRenderScheduler.Add(cloc);
        chunkBag.Add(chunk);
    }
}
