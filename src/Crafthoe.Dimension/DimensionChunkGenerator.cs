namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkGenerator(IChunkGenerator generator)
{
    public void Generate(Vector2i cloc) => generator.Generate(cloc);
}
