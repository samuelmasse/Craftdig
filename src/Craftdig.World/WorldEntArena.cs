namespace Craftdig;

[World]
public class WorldEntArena(WorldEntIdxContext context) : EntIdxArena(context)
{
    public override EntPtrIdx Alloc() => Alloc(Guid.NewGuid());

    public EntPtrIdx Alloc(Guid id) => base.Alloc().Mutate().Id(id);

    public EntPtrIdx AllocTransient() => base.Alloc();

    /// <summary>Invalidates scope-owned Ents before releasing their Indexed hooks.</summary>
    public override void Dispose()
    {
        base.Dispose();
        context.Dispose();
    }
}
