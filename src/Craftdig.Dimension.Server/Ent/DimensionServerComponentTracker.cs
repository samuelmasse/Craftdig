namespace Craftdig;

[Dimension]
public class DimensionServerComponentTracker<T, N>(
    DimensionEntScratched scratched,
    DimensionEntSyncCatalog catalog) : EntServerComponentTracker<T, N>(scratched, catalog)
    where N : IComponent;

[Dimension]
public class DimensionServerComponentArrayTracker<T, N>(
    DimensionEntScratched scratched,
    DimensionEntSyncCatalog catalog) : EntServerComponentArrayTracker<T, N>(scratched, catalog)
    where N : IComponent;
