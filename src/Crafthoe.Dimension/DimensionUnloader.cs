namespace Crafthoe.Dimension;

[DimensionLoader]
public class DimensionUnloader(
    DimensionChunkThreads chunkThreads,
    DimensionRegionThread regionThread,
    DimensionMetrics metrics,
    DimensionRegionInvalidation regionInvalidation)
{
    public void Run()
    {
        chunkThreads.Stop();
        regionInvalidation.Drain();
        regionThread.Stop();

        metrics.Stop();
    }
}
