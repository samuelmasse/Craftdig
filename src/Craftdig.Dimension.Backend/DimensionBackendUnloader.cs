namespace Craftdig.Dimension.Backend;

[DimensionLoader]
public class DimensionBackendUnloader(
    DimensionChunkThreads chunkThreads,
    DimensionRegionThread regionThread,
    DimensionEntRegionThread entRegionThread,
    DimensionRegionInvalidation regionInvalidation)
{
    public void Run()
    {
        chunkThreads.Stop();
        regionInvalidation.Drain();
        regionThread.Stop();
        entRegionThread.Stop();
    }
}
