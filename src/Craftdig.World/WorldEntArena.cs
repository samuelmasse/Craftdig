namespace Craftdig.World;

[World]
public class WorldEntArena(WorldEntIdxContextBuilder context) : EntIdxArena(context.Ent)
{
    public override EntPtrIdx Alloc() => Alloc(Guid.NewGuid());

    public EntPtrIdx Alloc(Guid id) => base.Alloc().Mutate().Id(id);

    public EntPtrIdx AllocTransient() => base.Alloc();
}
