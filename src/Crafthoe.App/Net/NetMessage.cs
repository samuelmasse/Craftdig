namespace Crafthoe.App;

public readonly ref struct NetMessage(int type, Span<byte> data)
{
    public readonly int Type = type;
    public readonly Span<byte> Data = data;
}
