namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionTerrainGenerator(ITerrainGenerator generator)
{
    public void Generate(ChunkBlocks blocks, Vector2i cloc) => generator.Generate(blocks, cloc);
}
