namespace Craftdig;

[Dimension]
public class DimensionLights(DimensionLightsRaw raw)
{
    public bool TryGet(Vec3i loc, out LightLevels levels) => raw.TryGet(loc, out levels);

    public LightLevels Get(Vec3i loc) => raw.TryGet(loc, out var levels) ? levels : default;
}
