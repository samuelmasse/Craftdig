namespace Craftdig.Dimension.Backend;

public interface ITerrainGenerator
{
    void Generate(ChunkBlocks blocks, Vec2i cloc);
}
