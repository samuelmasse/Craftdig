namespace Craftdig;

public readonly record struct LightLevels(byte Sky, byte Block)
{
    public byte Max => Math.Max(Sky, Block);
}
