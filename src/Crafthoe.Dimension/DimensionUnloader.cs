namespace Crafthoe.Dimension;

[DimensionLoader]
public class DimensionUnloader(
    DimensionRegionFileHandles regionFileHandles)
{
    public void Run()
    {
        regionFileHandles.Drain();
    }
}
