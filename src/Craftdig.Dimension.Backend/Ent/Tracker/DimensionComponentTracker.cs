namespace Craftdig;

[Dimension]
public class DimensionComponentTracker<T, N>(
    DimensionEntDirty dirty,
    WorldComponentIndex<T, N> index) : WorldComponentTracker<T, N>(dirty, index)
    where N : IComponent;

[Dimension]
public class DimensionComponentArrayTracker<T, N>(
    DimensionEntDirty dirty,
    WorldComponentIndex<T[], N> index) : WorldComponentArrayTracker<T, N>(dirty, index)
    where N : IComponent;
