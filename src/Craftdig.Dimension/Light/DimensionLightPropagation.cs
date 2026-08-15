namespace Craftdig;

[Dimension]
public class DimensionLightPropagation
{
    private readonly Vec3i[] directions =
    [
        (1, 0, 0),
        (-1, 0, 0),
        (0, 1, 0),
        (0, -1, 0),
        (0, 0, 1),
        (0, 0, -1)
    ];

    public ReadOnlySpan<Vec3i> Directions => directions;

    public byte Contribution(LightChannel channel, byte source, byte opacity, int fromZ, int toZ)
    {
        if (source == 0)
            return 0;

        int loss = opacity;
        if (channel != LightChannel.Sky || source != LightLevel.Max || toZ != fromZ - 1)
            loss = Math.Max(1, loss);

        return loss >= source ? (byte)0 : (byte)(source - loss);
    }
}
