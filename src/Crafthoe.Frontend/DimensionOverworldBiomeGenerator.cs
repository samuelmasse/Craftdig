
namespace Crafthoe.Frontend;

[Dimension]
public class DimensionOverworldBiomeGenerator(ModuleBlocks block, DimensionBlocks blocks) : IBiomeGenerator
{
    public void Generate(Vector2i cloc)
    {
        var mem = blocks.ChunkBlocks(cloc);
        var loc = cloc * SectionSize;

        bool wasAir = true;

        for (int y = 0; y < SectionSize; y++)
        {
            for (int x = 0; x < SectionSize; x++)
            {
                for (int z = HeightSize - 1; z >= 0; z--)
                {
                    int index = (z << (SectionBits * 2)) + (y << SectionBits) + x;
                    var block = mem[index];

                    if (block.IsSolid())
                    {
                        if (wasAir)
                            Generate((x + loc.X, y + loc.Y, z));

                        wasAir = false;
                    }
                    else wasAir = true;
                }
            }
        }
    }

    private void Generate(Vector3i loc)
    {
        blocks.TrySet(loc, (Ent)block.Grass);
        blocks.TrySet(loc - (0, 0, 1), (Ent)block.Dirt);
        blocks.TrySet(loc - (0, 0, 2), (Ent)block.Dirt);
    }
}
