namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionEntRegionThread(
    DimensionEntRegionFlusherThreads flusherThreads,
    DimensionEntRegionFileHandles fileHandles,
    DimensionEntPersister entPersister) : WorldEntRegionThread(flusherThreads, fileHandles, entPersister);
