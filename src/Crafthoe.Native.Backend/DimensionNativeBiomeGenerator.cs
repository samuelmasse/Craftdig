namespace Crafthoe.Native;

[Dimension]
public class DimensionNativeBiomeGenerator(ModuleNative m) : IBiomeGenerator
{
    public void Generate(Span<Ent> blocks, Vector2i cloc)
    {
        bool wasAir = true;

        for (int y = 0; y < SectionSize; y++)
        {
            for (int x = 0; x < SectionSize; x++)
            {
                for (int z = HeightSize - 1; z >= 0; z--)
                {
                    var block = blocks[new Vector3i(x, y, z).ToInnerIndex()];

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

    private void Generate(Span<Ent> blocks, Vector3i loc)
    {
        blocks[loc.ToInnerIndex()] = m.GrassBlock;
        blocks[(loc - (0, 0, 1)).ToInnerIndex()] = m.DirtBlock;
        blocks[(loc - (0, 0, 2)).ToInnerIndex()] = m.DirtBlock;
    }
}
