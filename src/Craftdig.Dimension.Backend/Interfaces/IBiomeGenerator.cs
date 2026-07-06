namespace Craftdig.Dimension.Backend;

public interface IBiomeGenerator
{
    void Generate(ChunkBlocks blocks, Vec2i cloc);
}
