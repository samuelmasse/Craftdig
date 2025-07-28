namespace Crafthoe.Dimension;

[Dimension]
public class DimensionGenerator(IChunkGenerator generator)
{
    public void Generate(Vector2i cloc) => generator.Generate(cloc);
}
