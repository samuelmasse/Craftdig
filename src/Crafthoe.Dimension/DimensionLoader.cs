namespace Crafthoe.Dimension;

[DimensionLoader]
public class DimensionLoader(
    DimensionMetrics metrics,
    DimensionChunkThreads chunkThreads,
    DimensionSectionThreads sectionThreads,
    DimensionRegionThread regionThread)
{
    public void Run()
    {
        metrics.Start();
        chunkThreads.Start();
        sectionThreads.Start();
        regionThread.Start();
    }
}
