namespace Crafthoe.Dimension;

[Dimension]
public class DimensionBiomeGenerator(IBiomeGenerator generator)
{
    public void Generate(Vector2i cloc) => generator.Generate(cloc);
}
