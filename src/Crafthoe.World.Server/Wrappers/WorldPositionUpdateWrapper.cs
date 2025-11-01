namespace Crafthoe.World.Server;

[World]
public class WorldPositionUpdateWrapper
{
    private readonly Vector3d[] buf = [default];

    public Span<byte> Wrap(Vector3d position)
    {
        buf[0] = position;
        return MemoryMarshal.AsBytes(buf.AsSpan());
    }
}
