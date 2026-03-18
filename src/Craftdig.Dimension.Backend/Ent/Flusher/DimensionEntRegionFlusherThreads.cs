namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionEntRegionFlusherThreads(
    DimensionEntRegionFlusherBag bag,
    DimensionEntRegionFlusherWorker worker) : WorldEntRegionFlusherThreads(bag, worker);
