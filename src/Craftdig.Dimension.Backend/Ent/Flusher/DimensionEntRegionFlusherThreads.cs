namespace Craftdig;

[Dimension]
public class DimensionEntRegionFlusherThreads(
    DimensionEntRegionFlusherBag bag,
    DimensionEntRegionFlusherWorker worker) : WorldEntRegionFlusherThreads(bag, worker);
