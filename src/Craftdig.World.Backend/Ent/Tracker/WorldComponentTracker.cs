namespace Craftdig.World.Backend;

public abstract class WorldComponentTracker
{
    public abstract void AddTo(EntIdxContextBuilder context);
}

[World]
public class WorldComponentTracker<T, N>(WorldEntDirty dirty, WorldComponentIndex<T, N> index) :
    WorldComponentTracker
    where T : IEquatable<T>
    where N : IComponent
{
    public override void AddTo(EntIdxContextBuilder context) => context.AddPre<T, N>(Intercept);

    private void Intercept(EntMutIdx ent, in T value)
    {
        var old = ent.Get<T, N>();
        if (value.Equals(old))
            return;

        dirty.Mark(ent, index.Index);
    }
}

[World]
public class WorldComponentArrayTracker<T, N>(WorldEntDirty dirty, WorldComponentIndex<T[], N> index) :
    WorldComponentTracker
    where T : IEquatable<T>
    where N : IComponent
{
    public override void AddTo(EntIdxContextBuilder context) => context.AddPre<T[], N>(Intercept);

    private void Intercept(EntMutIdx ent, in T[] value)
    {
        dirty.Mark(ent, index.Index);
    }
}
