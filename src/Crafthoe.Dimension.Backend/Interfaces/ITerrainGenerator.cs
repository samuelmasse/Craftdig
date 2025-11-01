namespace Crafthoe.Dimension;

public interface ITerrainGenerator
{
    void Generate(Span<Ent> blocks, Vector2i cloc);
}
