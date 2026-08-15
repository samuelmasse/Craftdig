namespace Craftdig;

[Dimension]
public class DimensionTerrainGenerator(ITerrainGenerator generator)
{
    public void Generate(ChunkBlocks blocks, Vec2i cloc) => generator.Generate(blocks, cloc);
}
