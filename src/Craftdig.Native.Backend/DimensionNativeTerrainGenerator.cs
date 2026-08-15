namespace Craftdig;

[Dimension]
public class DimensionNativeTerrainGenerator : ITerrainGenerator
{
    private const float NoiseMin = -0.5f;
    private const float NoiseMax = 1.5f;

    private const float BiasCenterZ = 60f;
    private const float BiasScale = 30f;

    private static readonly int StoneMaxZ = (int)MathF.Floor(BiasCenterZ + BiasScale * NoiseMin);
    private static readonly int AirMinZ = (int)MathF.Ceiling(BiasCenterZ + BiasScale * NoiseMax);

    private readonly ModuleNative m;
    private readonly DimensionNativeNoise noise;

    public DimensionNativeTerrainGenerator(
        ModuleNative m,
        DimensionNativeNoise noise,
        DimensionBackendUnloaderHandlers backendUnloaderHandlers)
    {
        this.m = m;
        this.noise = noise;
        backendUnloaderHandlers.Add(noise.Dispose);
    }

    public void Generate(ChunkBlocks blocks, Vec2i cloc)
    {
        var loc = cloc * SectionSize;
        Span<float> noiseValues = stackalloc float[SectionVolume];

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
                noise.GenerateSection(noiseValues, loc.X, loc.Y, minZ);
                var section = blocks.Slice(sz);

                for (int z = 0; z < SectionSize; z++)
                {
                    int zIndex = z << (SectionBits * 2);
                    int worldZ = z + minZ;

                    for (int y = 0; y < SectionSize; y++)
                    {
                        int yzIndex = zIndex + (y << SectionBits);
                        int worldY = loc.Y + y;

                        for (int x = 0; x < SectionSize; x++)
                        {
                            int index = yzIndex + x;
                            section[index] = Generate(loc.X + x, worldY, worldZ, noiseValues[index]);
                        }
                    }
                }
            }
        }
    }

    private Ent Generate(int x, int y, int z, float noiseValue)
    {
        if (x == 0 && y == 0)
            return m.StoneBlock;

        if (z <= StoneMaxZ)
            return m.StoneBlock;

        if (z >= AirMinZ)
            return m.AirBlock;

        float bias = (z - BiasCenterZ) / BiasScale;
        float n = noiseValue + 0.5f;

        if (n - bias >= 0)
            return m.StoneBlock;

        return m.AirBlock;
    }
}
