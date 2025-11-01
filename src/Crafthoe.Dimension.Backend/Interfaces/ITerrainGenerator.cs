namespace Crafthoe.Dimension.Backend;

public interface ITerrainGenerator
{
    void Generate(Span<Ent> blocks, Vector2i cloc);
}
