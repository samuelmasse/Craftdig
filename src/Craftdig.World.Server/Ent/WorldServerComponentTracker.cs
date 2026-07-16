namespace Craftdig.World.Server;

public abstract class WorldServerComponentTracker
{
    public abstract void AddTo(EntIdxContextBuilder context);
}

public abstract class EntServerComponentTracker<T, N>(EntScratched scratched, EntSyncCatalog catalog) :
    WorldServerComponentTracker
    where T : IEquatable<T>
    where N : IComponent
{
    private readonly int index = catalog[N.Component].Ordinal;

    public override void AddTo(EntIdxContextBuilder context)
    {
        context.AddPre<T, N>(Intercept);
        context.AddPost<T, N>(Intercept);
    }

    private void Intercept(EntMutIdx ent, in T value)
    {
        if (!ent.Has<T, N>() || !value.Equals(ent.Get<T, N>()))
            scratched.Mark(ent, index);
    }

    private void Intercept(EntMutIdx ent)
    {
        if (!ent.Has<T, N>())
            scratched.Mark(ent, index);
    }
}

[World]
public class WorldServerComponentTracker<T, N>(WorldEntScratched scratched, WorldEntSyncCatalog catalog) :
    EntServerComponentTracker<T, N>(scratched, catalog)
    where T : IEquatable<T>
    where N : IComponent;

public abstract class EntServerComponentArrayTracker<T, N>(EntScratched scratched, EntSyncCatalog catalog) :
    WorldServerComponentTracker
    where T : IEquatable<T>
    where N : IComponent
{
    private readonly int index = catalog[N.Component].Ordinal;

    public override void AddTo(EntIdxContextBuilder context)
    {
        context.AddPre<T[], N>(Intercept);
        context.AddPost<T[], N>(Intercept);
    }

    private void Intercept(EntMutIdx ent, in T[] value) => scratched.Mark(ent, index);

    private void Intercept(EntMutIdx ent)
    {
        if (!ent.Has<T[], N>())
            scratched.Mark(ent, index);
    }
}

[World]
public class WorldServerComponentArrayTracker<T, N>(WorldEntScratched scratched, WorldEntSyncCatalog catalog) :
    EntServerComponentArrayTracker<T, N>(scratched, catalog)
    where T : IEquatable<T>
    where N : IComponent;
