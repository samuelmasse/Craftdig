namespace Crafthoe.Dimension;

[Dimension]
public class DimensionTerrainGenerator(ITerrainGenerator generator)
{
    public void Generate(Vector2i cloc) => generator.Generate(cloc);
}
