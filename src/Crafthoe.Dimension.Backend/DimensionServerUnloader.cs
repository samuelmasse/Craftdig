namespace Crafthoe.Dimension.Backend;

[DimensionLoader]
public class DimensionServerUnloader(
    DimensionChunkThreads chunkThreads,
    DimensionRegionThread regionThread,
    DimensionRegionInvalidation regionInvalidation)
{
    public void Run()
    {
        chunkThreads.Stop();
        regionInvalidation.Drain();
        regionThread.Stop();
    }
}
