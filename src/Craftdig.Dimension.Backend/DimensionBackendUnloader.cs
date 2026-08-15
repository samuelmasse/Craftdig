namespace Craftdig;

[DimensionLoader]
public class DimensionBackendUnloader(
    DimensionChunkThreads chunkThreads,
    DimensionBackendUnloaderHandlers backendUnloaderHandlers,
    DimensionRegionThread regionThread,
    DimensionEntRegionThread entRegionThread,
    DimensionRegionInvalidation regionInvalidation)
{
    public void Run()
    {
        chunkThreads.Stop();
        backendUnloaderHandlers.Run();
        regionInvalidation.Drain();
        regionThread.Stop();
        entRegionThread.Stop();
    }
}
