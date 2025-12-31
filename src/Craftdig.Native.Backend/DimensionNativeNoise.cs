namespace Craftdig.Native;

[Dimension]
public class DimensionNativeNoise
{
    private readonly FastNoiseLite generator;

    public FastNoiseLite Generator => generator;

    public DimensionNativeNoise(WorldMeta meta)
    {
        generator = new();
        generator.SetFractalType(FastNoiseLite.FractalType.FBm);
        generator.SetSeed(meta.Seed);
    }
}
