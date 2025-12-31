namespace Craftdig.Native;

[Dimension]
public class DimensionNativeBiomeGenerator(ModuleNative m) : IBiomeGenerator
{
    public void Generate(ChunkBlocks blocks, Vector2i cloc)
    {
        int maxZ = FindMaxZ(blocks);
        bool wasAir = true;

        for (int y = 0; y < SectionSize; y++)
        {
            for (int x = 0; x < SectionSize; x++)
            {
                for (int z = maxZ; z >= 0; z--)
                {
                    var block = blocks[(x, y, z)];

                    if (block.IsSolid())
                    {
                        if (wasAir)
                            Generate(blocks, (x, y, z));

                        wasAir = false;
                    }
                    else wasAir = true;
                }
            }
        }
    }

    private int FindMaxZ(ChunkBlocks blocks)
    {
        for (int sz = SectionHeight - 1; sz >= 0; sz--)
        {
            if (blocks.Uniform(sz) == default || blocks.Uniform(sz).IsSolid())
                return (sz + 1) * SectionSize - 1;
        }

        return 0;
    }

    private void Generate(ChunkBlocks blocks, Vector3i loc)
    {
        blocks[loc] = m.GrassBlock;
        blocks[(loc - (0, 0, 1))] = m.DirtBlock;
        blocks[(loc - (0, 0, 2))] = m.DirtBlock;
    }
}
