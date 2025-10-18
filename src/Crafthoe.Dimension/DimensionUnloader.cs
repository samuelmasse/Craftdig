namespace Crafthoe.Dimension;

[DimensionLoader]
public class DimensionUnloader(
    DimensionChunkThreads chunkThreads,
    DimensionSectionThreads sectionThreads,
    DimensionRegionThread regionThread,
    DimensionMetrics metrics,
    DimensionRegionInvalidation regionInvalidation)
{
    public void Run()
    {
        chunkThreads.Stop();
        sectionThreads.Stop();

        regionInvalidation.Drain();
        regionThread.Stop();

        metrics.Stop();
    }
}
