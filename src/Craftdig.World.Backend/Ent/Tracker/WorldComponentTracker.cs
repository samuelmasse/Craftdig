namespace Craftdig.World.Backend;

public abstract class WorldComponentTracker
{
    public abstract void AddTo(EntIdxContextBuilder context);
}

[World]
public class WorldComponentTracker<T, N>(
    AppLog log,
    WorldEntDirty dirty,
    WorldComponentIndex<T, N> index) : WorldComponentTracker where T : IEquatable<T>
{
    public override void AddTo(EntIdxContextBuilder context) => context.AddPre<T, N>(Intercept);

    private void Intercept(EntMutIdx ent, T value)
    {
        var old = ent.Get<T, N>();
        if (value.Equals(old))
            return;

        dirty.Mark(ent, index.Index);
        log.Info("New value {0} ({1} -> {2}) for {3}", typeof(N).Name, old, value, ent.Id);
    }
}

[World]
public class WorldComponentArrayTracker<T, N>(
    AppLog log,
    WorldEntDirty dirty,
    WorldComponentIndex<T[], N> index) : WorldComponentTracker where T : IEquatable<T>
{
    public override void AddTo(EntIdxContextBuilder context) => context.AddPre<T[], N>(Intercept);

    private void Intercept(EntMutIdx ent, T[] value)
    {
        dirty.Mark(ent, index.Index);
        log.Info("New array {0} for {1}", typeof(N).Name, ent.Id);
    }
}
