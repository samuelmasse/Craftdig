namespace Craftdig.Protocol;

public readonly ref struct NetMessage(ushort type, ReadOnlySpan<byte> data)
{
    public readonly ushort Type = type;
    public readonly ReadOnlySpan<byte> Data = data;
}
