namespace Crafthoe.Dimension;

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
            chunkGenerator.Generate(input.Blocks.Span, input.Cloc);
            biomeGenerator.Generate(input.Blocks.Span, input.Cloc);
        }

        output.Add(input);
    }
}
