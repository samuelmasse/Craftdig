namespace Craftdig.Dimension.Backend;

public abstract class DimensionComponentTracker
{
    public abstract void AddTo(EntIdxContextBuilder context);
}

[Dimension]
public class DimensionComponentTracker<T, N>(
    AppLog log,
    DimensionEntDirty dirty,
    DimensionComponentIndex<T, N> index) : DimensionComponentTracker where T : IEquatable<T>
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

[Dimension]
public class DimensionComponentArrayTracker<T, N>(
    AppLog log,
    DimensionEntDirty dirty,
    DimensionComponentIndex<T[], N> index) : DimensionComponentTracker where T : IEquatable<T>
{
    public override void AddTo(EntIdxContextBuilder context) => context.AddPre<T[], N>(Intercept);

    private void Intercept(EntMutIdx ent, T[] value)
    {
        dirty.Mark(ent, index.Index);
        log.Info("New array {0} for {1}", typeof(N).Name, ent.Id);
    }
}
