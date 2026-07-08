namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionComponentTracker<T, N>(
    DimensionEntDirty dirty,
    WorldComponentIndex<T, N> index) : WorldComponentTracker<T, N>(dirty, index)
    where T : IEquatable<T>
    where N : IComponent;

[Dimension]
public class DimensionComponentArrayTracker<T, N>(
    DimensionEntDirty dirty,
    WorldComponentIndex<T[], N> index) : WorldComponentArrayTracker<T, N>(dirty, index)
    where T : IEquatable<T>
    where N : IComponent;
