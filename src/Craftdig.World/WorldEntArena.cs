namespace Craftdig.World;

[World]
public class WorldEntArena(WorldEntIdxContextBuilder context) : EntIdxArena(context.Ent)
{
    public override EntPtrIdx Alloc() => base.Alloc().Mutate().Id(Guid.NewGuid());
}
