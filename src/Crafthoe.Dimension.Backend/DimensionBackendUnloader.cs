namespace Crafthoe.Dimension.Backend;

[DimensionLoader]
public class DimensionBackendUnloader(
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
