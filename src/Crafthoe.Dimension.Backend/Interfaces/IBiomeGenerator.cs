namespace Crafthoe.Dimension;

public interface IBiomeGenerator
{
    void Generate(Span<Ent> blocks, Vector2i cloc);
}
