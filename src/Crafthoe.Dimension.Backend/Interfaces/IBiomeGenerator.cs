namespace Crafthoe.Dimension.Backend;

public interface IBiomeGenerator
{
    void Generate(ChunkBlocks blocks, Vector2i cloc);
}
