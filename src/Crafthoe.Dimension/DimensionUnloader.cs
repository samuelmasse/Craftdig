namespace Crafthoe.Dimension;

[DimensionLoader]
public class DimensionUnloader(
    DimensionRegionInvalidation regionInvalidation,
    DimensionRegionFileHandles regionFileHandles)
{
    public void Run()
    {
        regionInvalidation.Drain();
        regionFileHandles.Drain();
    }
}
