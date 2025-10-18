namespace Crafthoe.Native;

[Dimension]
public class DimensionNativeBiomeGenerator(ModuleNative m, DimensionBlocksRaw blocksRaw) : IBiomeGenerator
{
    public void Generate(Span<Ent> blocks, Vector2i cloc)
    {
        var loc = cloc * SectionSize;

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
        blocksRaw.TrySet(loc, (Ent)m.GrassBlock);
        blocksRaw.TrySet(loc - (0, 0, 1), (Ent)m.DirtBlock);
        blocksRaw.TrySet(loc - (0, 0, 2), (Ent)m.DirtBlock);
    }
}
