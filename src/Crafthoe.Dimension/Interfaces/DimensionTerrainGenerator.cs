namespace Crafthoe.Dimension;

[Dimension]
public class DimensionTerrainGenerator(ITerrainGenerator generator)
{
    public void Generate(Span<Ent> blocks, Vector2i cloc) => generator.Generate(blocks, cloc);
}
