namespace Crafthoe.Dimension;

[DimensionLoader]
public class DimensionUnloader(
    DimensionSectionThreads sectionThreads,
    DimensionMetrics metrics,
    DimensionRegionInvalidation regionInvalidation,
    DimensionRegionFileHandles regionFileHandles)
{
    public void Run()
    {
        sectionThreads.Stop();
        metrics.Stop();
        regionInvalidation.Drain();
        regionFileHandles.Drain();
    }
}
