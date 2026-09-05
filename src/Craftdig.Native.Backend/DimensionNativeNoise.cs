namespace Craftdig;

/// <summary>Samples one immutable typed terrain graph into independent worker buffers.</summary>
[Dimension]
public class DimensionNativeNoise
{
    private const float NoiseFrequency = 0.01f;
    private const float NoiseFeatureScale = 1f / NoiseFrequency;
    private const int FractalOctaves = 3;
    private const float FractalLacunarity = 2f;
    private const float FractalGain = 0.5f;
    private const float FractalBounding = 1f / (1f + FractalGain + (FractalGain * FractalGain));

    private static readonly float[] RotatedSectionX = CreateRotatedSectionPositions(0);
    private static readonly float[] RotatedSectionY = CreateRotatedSectionPositions(1);
    private static readonly float[] RotatedSectionZ = CreateRotatedSectionPositions(2);

    /// <summary>Retains this dimension's terrain nodes independently of the shared FnGraph service.</summary>
    private readonly FnGraphNode root;
    private readonly int seed;

    /// <summary>Configures and warms the complete graph before terrain workers can sample it concurrently.</summary>
    public DimensionNativeNoise(WorldMeta meta, FnGraph graph)
    {
        seed = meta.Seed;
        var source = graph.Create(FnNodeType.Simplex)
            .Float(FnFloatVariable.FeatureScale, NoiseFeatureScale)
            .Integer(FnIntegerVariable.SeedOffset, 0)
            .Float(FnFloatVariable.OutputMinimum, -FractalBounding)
            .Float(FnFloatVariable.OutputMaximum, FractalBounding);
        root = graph.Create(FnNodeType.FractalFbm)
            .Integer(FnIntegerVariable.Octaves, FractalOctaves)
            .Float(FnFloatVariable.Lacunarity, FractalLacunarity)
            .Hybrid(FnHybrid.Gain, FractalGain)
            .Hybrid(FnHybrid.WeightedStrength, 0f)
            .Source(FnSource.Source, source);
        Span<float> warmup = stackalloc float[1];
        root.GenPositionArray3D(warmup, RotatedSectionX, RotatedSectionY, RotatedSectionZ, Vec3.Zero, seed);
    }

    /// <summary>Preserves the terrain's rotated sample positions and writes exactly one section.</summary>
    public void GenerateSection(Span<float> noiseOut, int x, int y, int z)
    {
        TransformTerrainNoise3D(x, y, z, out var offsetX, out var offsetY, out var offsetZ);
        root.GenPositionArray3D(noiseOut[..SectionVolume], RotatedSectionX, RotatedSectionY, RotatedSectionZ,
            (offsetX, offsetY, offsetZ), seed);
    }

    private static float[] CreateRotatedSectionPositions(int component)
    {
        var positions = new float[SectionVolume];

        for (int z = 0; z < SectionSize; z++)
        {
            int zIndex = z << (SectionBits * 2);
            for (int y = 0; y < SectionSize; y++)
            {
                int yzIndex = zIndex + (y << SectionBits);
                for (int x = 0; x < SectionSize; x++)
                {
                    TransformTerrainNoise3D(x, y, z, out var tx, out var ty, out var tz);

                    positions[yzIndex + x] = component switch
                    {
                        0 => tx,
                        1 => ty,
                        _ => tz,
                    };
                }
            }
        }

        return positions;
    }

    private static void TransformTerrainNoise3D(float x, float y, float z, out float tx, out float ty, out float tz)
    {
        const float r3 = 2f / 3f;
        var r = (x + y + z) * r3;
        tx = r - x;
        ty = r - y;
        tz = r - z;
    }
}
