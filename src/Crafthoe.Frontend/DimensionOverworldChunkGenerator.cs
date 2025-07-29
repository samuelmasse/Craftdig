
namespace Crafthoe.Frontend;

[Dimension]
public class DimensionOverworldChunkGenerator(ModuleBlocks block, DimensionBlocks blocks) : IChunkGenerator
{
    private readonly FastNoiseLite noise = new();

    public void Generate(Vector2i cloc)
    {
        var mem = blocks.ChunkBlocks(cloc);
        var sloc = cloc * SectionSize;

        for (int y = 0; y < SectionSize; y++)
        {
            for (int x = 0; x < SectionSize; x++)
            {
                int h = (int)(noise.GetNoise(sloc.X + x, sloc.Y + y) * 15) + 60;

                for (int z = 0; z < HeightSize; z++)
                {
                    int index = (z << (SectionBits * 2)) + (y << SectionBits) + x;
                    mem[index] = Generate(z, h);
                }
            }
        }
    }

    private ReadOnlyEntity Generate(int z, float h)
    {
        if (z < h - 5)
            return block.Stone;
        else if (z < h)
            return block.Dirt;
        else return block.Air;
    }
}
