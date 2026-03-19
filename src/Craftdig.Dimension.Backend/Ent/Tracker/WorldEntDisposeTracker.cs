namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionEntDisposeTracker(DimensionEntPersister entPersister) : WorldEntDisposeTracker(entPersister);
