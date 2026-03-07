namespace Craftdig.World;

public class EntIdxArena(EntObj context)
{
    private readonly EntArena arena = new();

    public int Allocated => arena.Allocated;

    public virtual EntPtrIdx Alloc()
    {
        var ent = new EntPtrIdx(arena.Alloc(), (Ent)context);
        context.ContextEntLiveSet.Add(ent);
        return ent;
    }

    public virtual void Dispose()
    {
        foreach (var ent in context.ContextEntLiveSet.ToArray())
            ent.Dispose();

        arena.Dispose();
    }
}
