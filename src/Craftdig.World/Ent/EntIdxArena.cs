namespace Craftdig.World;

public class EntIdxArena(EntObj context)
{
    private readonly EntArena arena = new();

    public int Allocated => arena.Allocated;

    public virtual EntPtrIdx Alloc() => new(arena.Alloc(), (Ent)context);
    public virtual void Dispose() => arena.Dispose();
}
