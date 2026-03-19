namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionEntPersister(DimensionEntRegionWriter entRegionWriter) : WorldEntPersister(entRegionWriter);
