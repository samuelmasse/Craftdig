namespace Crafthoe.Dimension;

[DimensionLoader]
public class DimensionUnloader(
    DimensionMetrics metrics,
    DimensionRegionInvalidation regionInvalidation,
    DimensionRegionFileHandles regionFileHandles)
{
    public void Run()
    {
        metrics.Stop();
        regionInvalidation.Drain();
        regionFileHandles.Drain();
    }
}
