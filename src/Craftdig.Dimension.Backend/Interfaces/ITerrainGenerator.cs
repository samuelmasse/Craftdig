namespace Craftdig;

public interface ITerrainGenerator
{
    void Generate(ChunkBlocks blocks, Vec2i cloc);
}
