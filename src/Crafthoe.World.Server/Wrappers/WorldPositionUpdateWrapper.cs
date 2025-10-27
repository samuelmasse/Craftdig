namespace Crafthoe.World;

[World]
public class WorldPositionUpdateWrapper
{
    public const int Type = 3;

    private readonly Vector3d[] buf = [default];

    public NetMessage Wrap(Vector3d position)
    {
        buf[0] = position;
        return new(Type, MemoryMarshal.AsBytes(buf.AsSpan()));
    }
}
