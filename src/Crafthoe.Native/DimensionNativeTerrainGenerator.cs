namespace Crafthoe.Native;

[Dimension]
public class DimensionNativeTerrainGenerator(ModuleNative m, DimensionNativeNoise noise) : ITerrainGenerator
{
    public void Generate(Span<Ent> blocks, Vector2i cloc)
    {
        var loc = cloc * SectionSize;

        for (int y = 0; y < SectionSize; y++)
            for (int x = 0; x < SectionSize; x++)
                for (int z = 0; z < HeightSize; z++)
                    blocks[new Vector3i(x, y, z).ToInnerIndex()] = Generate((loc.X + x, loc.Y + y, z));
    }

    private Ent Generate(Vector3i loc)
    {
        if (loc.X == 0 && loc.Y == 0)
            return m.StoneBlock;

        float bias = ((loc.Z - 60) / 30f);
        if (bias >= 1.5f)
            return loc.Z < 60 ? m.StoneBlock : m.AirBlock;
        float n = noise.Generator.GetNoise(loc.X, loc.Y, loc.Z) + 0.5f;

        if (n - bias > 0)
            return m.StoneBlock;

        return m.AirBlock;
    }
}
