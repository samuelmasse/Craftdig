namespace Craftdig;

public abstract class WorldServerComponentTracker
{
    public abstract void AddTo(EntIdxContext context);
}

public abstract class EntServerComponentTracker<T, N>(EntScratched scratched, EntSyncCatalog catalog) :
    WorldServerComponentTracker
    where N : IComponent
{
    private readonly int index = catalog[N.Component].Ordinal;

    public override void AddTo(EntIdxContext context)
    {
        context.OnChange<T, N>(Track);
    }

    private void Track(EntMutIdx ent, in EntChange<T> change) => scratched.Mark(ent, index);
}

[World]
public class WorldServerComponentTracker<T, N>(WorldEntScratched scratched, WorldEntSyncCatalog catalog) :
    EntServerComponentTracker<T, N>(scratched, catalog)
    where N : IComponent;

public abstract class EntServerComponentArrayTracker<T, N>(EntScratched scratched, EntSyncCatalog catalog) :
    WorldServerComponentTracker
    where N : IComponent
{
    private readonly int index = catalog[N.Component].Ordinal;

    public override void AddTo(EntIdxContext context)
    {
        context.OnWrite<T[], N>(Track);
    }

    private void Track(EntMutIdx ent) => scratched.Mark(ent, index);
}

[World]
public class WorldServerComponentArrayTracker<T, N>(WorldEntScratched scratched, WorldEntSyncCatalog catalog) :
    EntServerComponentArrayTracker<T, N>(scratched, catalog)
    where N : IComponent;
