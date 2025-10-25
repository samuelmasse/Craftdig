namespace Crafthoe.Dimension;

[DimensionLoader]
public class DimensionLoader(
    DimensionMetrics metrics,
    DimensionChunkThreads chunkThreads,
    DimensionRegionThread regionThread)
{
    public void Run()
    {
        metrics.Start();
        chunkThreads.Start();
        regionThread.Start();
    }
}
