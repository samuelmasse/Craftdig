namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionBiomeGenerator(IBiomeGenerator generator)
{
    public void Generate(ChunkBlocks blocks, Vec2i cloc) => generator.Generate(blocks, cloc);
}
