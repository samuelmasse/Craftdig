namespace Crafthoe.World;

[World]
public class WorldChunkUpdateWrapper
{
    public const int Type = 4;

    public NetMessage Wrap(Span<byte> data)
    {
        return new(Type, data);
    }
}
