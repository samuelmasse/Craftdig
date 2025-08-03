
namespace Crafthoe.Frontend;

[Dimension]
public class DimensionOverworldChunkGenerator(ModuleBlocks block, DimensionBlocks blocks) : IChunkGenerator
{
    private readonly FastNoiseLite noise = new();

    public void Generate(Vector2i cloc)
    {
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);

        var mem = blocks.ChunkBlocks(cloc);
        var loc = cloc * SectionSize;

        for (int y = 0; y < SectionSize; y++)
        {
            for (int x = 0; x < SectionSize; x++)
            {
                for (int z = 0; z < HeightSize; z++)
                {
                    int index = (z << (SectionBits * 2)) + (y << SectionBits) + x;
                    mem[index] = (Ent)Generate((loc.X + x, loc.Y + y, z));
                }
            }
        }
    }

    private EntRef Generate(Vector3i loc)
    {
        if (loc.X == 0 && loc.Y == 0)
            return block.Stone;

        if (loc.Z < 45)
            return block.Stone;

        if (loc.Z >= 105)
            return block.Air;

        float n = noise.GetNoise(loc.X, loc.Y, loc.Z) + 0.5f;

        if (n - ((loc.Z - 60) / 30f) > 0)
            return block.Stone;

        return block.Air;
    }
}
