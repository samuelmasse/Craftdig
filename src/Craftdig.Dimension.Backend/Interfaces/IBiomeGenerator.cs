namespace Craftdig;

public interface IBiomeGenerator
{
    void Generate(ChunkBlocks blocks, Vec2i cloc);
}
