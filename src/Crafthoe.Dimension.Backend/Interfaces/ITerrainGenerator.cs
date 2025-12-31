namespace Craftdig.Dimension.Backend;

public interface ITerrainGenerator
{
    void Generate(ChunkBlocks blocks, Vector2i cloc);
}
