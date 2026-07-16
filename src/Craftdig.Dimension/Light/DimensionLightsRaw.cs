namespace Craftdig.Dimension;

[Dimension]
public class DimensionLightsRaw(DimensionEnt dimension, DimensionChunks chunks)
{
    public bool TryGet(Vec3i loc, out LightLevels levels)
    {
        if (!TryGetChunkLight(loc.Xy.ToCloc(), out var light))
        {
            levels = default;
            return false;
        }

        if (loc.Z == HeightSize)
        {
            levels = new(dimension.HasSkyLight ? LightLevel.Max : (byte)0, 0);
            return true;
        }

        if ((uint)loc.Z >= HeightSize)
        {
            levels = default;
            return false;
        }

        levels = new(light.Sky(loc), light.Block(loc));
        return true;
    }

    public bool TryGetChunkLight(Vec2i cloc, [NotNullWhen(true)] out ChunkLight? light)
    {
        chunks.TryGet(cloc, out var chunk);
        light = chunk.ChunkLight;
        return light != null;
    }
}
