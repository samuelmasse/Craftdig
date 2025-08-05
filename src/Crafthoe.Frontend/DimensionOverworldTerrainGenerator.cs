
namespace Crafthoe.Frontend;

[Dimension]
public class DimensionOverworldTerrainGenerator(ModuleBlocks block, DimensionBlocksRaw blocksRaw) : ITerrainGenerator
{
    private readonly FastNoiseLite noise = new();

    public void Generate(Vector2i cloc)
    {
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);

        var mem = blocksRaw.Span(cloc);
        var loc = cloc * SectionSize;

        for (int y = 0; y < SectionSize; y++)
            for (int x = 0; x < SectionSize; x++)
                for (int z = 0; z < HeightSize; z++)
                    mem[new Vector3i(x, y, z).ToInnerIndex()] = Generate((loc.X + x, loc.Y + y, z));
    }

    private Ent Generate(Vector3i loc)
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
