namespace Crafthoe.Dimension.Backend;

[Dimension]
public class DimensionBiomeGenerator(IBiomeGenerator generator)
{
    public void Generate(Span<Ent> blocks, Vector2i cloc) => generator.Generate(blocks, cloc);
}
