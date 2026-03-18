namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionComponentTracker<T, N>(
    AppLog log,
    DimensionEntDirty dirty,
    WorldComponentIndex<T, N> index) : WorldComponentTracker<T, N>(log, dirty, index) where T : IEquatable<T>;

[Dimension]
public class DimensionComponentArrayTracker<T, N>(
    AppLog log,
    DimensionEntDirty dirty,
    WorldComponentIndex<T[], N> index) : WorldComponentArrayTracker<T, N>(log, dirty, index) where T : IEquatable<T>;
