namespace Craftdig;

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
    public override void AddTo(EntIdxContextBuilder context)
    {
        context.AddPre<T, N>(Intercept);
        context.AddPost<T, N>(Intercept);
    }

    private void Intercept(EntMutIdx ent, in T value)
    {
        if (!ent.Has<T, N>() || !value.Equals(ent.Get<T, N>()))
            dirty.Mark(ent, index.Index);
    }

    private void Intercept(EntMutIdx ent)
    {
        if (!ent.Has<T, N>())
            dirty.Mark(ent, index.Index);
    }
}

[World]
public class WorldComponentArrayTracker<T, N>(WorldEntDirty dirty, WorldComponentIndex<T[], N> index) :
    WorldComponentTracker
    where T : IEquatable<T>
    where N : IComponent
{
    public override void AddTo(EntIdxContextBuilder context)
    {
        context.AddPre<T[], N>(Intercept);
        context.AddPost<T[], N>(Intercept);
    }

    private void Intercept(EntMutIdx ent, in T[] value) => dirty.Mark(ent, index.Index);

    private void Intercept(EntMutIdx ent)
    {
        if (!ent.Has<T[], N>())
            dirty.Mark(ent, index.Index);
    }
}
