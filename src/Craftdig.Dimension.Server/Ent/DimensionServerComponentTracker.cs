namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionServerComponentTracker<T, N>(
    DimensionEntScratched scratched,
    DimensionEntSyncCatalog catalog) : EntServerComponentTracker<T, N>(scratched, catalog)
    where T : IEquatable<T>
    where N : IComponent;

[Dimension]
public class DimensionServerComponentArrayTracker<T, N>(
    DimensionEntScratched scratched,
    DimensionEntSyncCatalog catalog) : EntServerComponentArrayTracker<T, N>(scratched, catalog)
    where T : IEquatable<T>
    where N : IComponent;
