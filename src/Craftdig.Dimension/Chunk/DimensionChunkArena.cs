namespace Craftdig.Dimension;

[Dimension]
public class DimensionChunkArena
{
    private readonly EntArena arena = new();

    public int Allocated => arena.Allocated;

    public virtual EntPtr Alloc() => arena.Alloc();
    public virtual void Dispose() => arena.Dispose();
}
