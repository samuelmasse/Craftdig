namespace Craftdig;

[Dimension]
public class DimensionChunkThreadWorker(
    DimensionTerrainGenerator chunkGenerator,
    DimensionBiomeGenerator biomeGenerator,
    DimensionChunkLightBuilder lightBuilder,
    DimensionChunkThreadOutputBag output)
{
    public void Work(ChunkThreadInput input)
    {
        if (!input.Noop)
        {
            chunkGenerator.Generate(input.Blocks, input.Cloc);
            biomeGenerator.Generate(input.Blocks, input.Cloc);

            for (int sz = 0; sz < SectionHeight; sz++)
                input.Blocks.Pack(sz);
        }

        var light = lightBuilder.Build(input.Blocks, input.Cloc);
        output.Add(new(input.Blocks, input.Cloc, input.Noop, light));
    }
}
