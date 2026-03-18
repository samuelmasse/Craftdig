namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionEntPersister(AppLog log, DimensionEntRegionWriter entRegionWriter) : WorldEntPersister(log, entRegionWriter);
