namespace Craftdig;

public abstract class WorldComponentTracker
{
    public abstract void AddTo(EntIdxContext context);
}

[World]
public class WorldComponentTracker<T, N>(WorldEntDirty dirty, WorldComponentIndex<T, N> index) :
    WorldComponentTracker
    where N : IComponent
{
    public override void AddTo(EntIdxContext context)
    {
        context.OnChange<T, N>(Track);
    }

    private void Track(EntMutIdx ent, in EntChange<T> change) => dirty.Mark(ent, index.Index);
}

[World]
public class WorldComponentArrayTracker<T, N>(WorldEntDirty dirty, WorldComponentIndex<T[], N> index) :
    WorldComponentTracker
    where N : IComponent
{
    public override void AddTo(EntIdxContext context)
    {
        context.OnWrite<T[], N>(Track);
    }

    private void Track(EntMutIdx ent) => dirty.Mark(ent, index.Index);
}
