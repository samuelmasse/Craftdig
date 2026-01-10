namespace Craftdig.Native;

[Dimension]
public class DimensionNativeTerrainGenerator(ModuleNative m, DimensionNativeNoise noise) : ITerrainGenerator
{
    private const float NoiseMin = -0.5f;
    private const float NoiseMax = 1.5f;

    private const float BiasCenterZ = 60f;
    private const float BiasScale = 30f;

    private static readonly int StoneMaxZ = (int)MathF.Floor(BiasCenterZ + BiasScale * NoiseMin);
    private static readonly int AirMinZ = (int)MathF.Ceiling(BiasCenterZ + BiasScale * NoiseMax);

    public void Generate(ChunkBlocks blocks, Vector2i cloc)
    {
        var loc = cloc * SectionSize;

        for (int sz = 0; sz < SectionHeight; sz++)
        {
            int minZ = sz * SectionSize;
            int maxZ = minZ + SectionSize;

            if (StoneMaxZ > maxZ)
                blocks.Fill(sz, m.StoneBlock);
            else if (AirMinZ < minZ)
                blocks.Fill(sz, m.AirBlock);
            else
            {
                for (int y = 0; y < SectionSize; y++)
                    for (int x = 0; x < SectionSize; x++)
                        for (int z = 0; z < SectionSize; z++)
                            blocks[(x, y, z + minZ)] = Generate((loc.X + x, loc.Y + y, z + minZ));
            }
        }
    }

    private Ent Generate(Vector3i loc)
    {
        if (loc.X == 0 && loc.Y == 0)
            return m.StoneBlock;

        if (loc.Z <= StoneMaxZ)
            return m.StoneBlock;

        if (loc.Z >= AirMinZ)
            return m.AirBlock;

        float bias = (loc.Z - BiasCenterZ) / BiasScale;
        float n = noise.Generator.GetNoise(loc.X, loc.Y, loc.Z) + 0.5f;

        if (n - bias >= 0)
            return m.StoneBlock;

        return m.AirBlock;
    }
}
