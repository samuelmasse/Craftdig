namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionChunkThreadWorker(
    DimensionTerrainGenerator chunkGenerator,
    DimensionBiomeGenerator biomeGenerator,
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

        output.Add(input);
    }
}
