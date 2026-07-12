namespace Craftdig.Dimension.Backend;

[DimensionLoader]
public class DimensionBackendUnloader(
    DimensionChunkThreads chunkThreads,
    DimensionBackendUnloaderHandlers backendUnloaderHandlers,
    DimensionRegionThread regionThread,
    DimensionEntRegionThread entRegionThread,
    DimensionRegionInvalidation regionInvalidation,
    DimensionLighting lighting)
{
    public void Run()
    {
        lighting.Stop();
        chunkThreads.Stop();
        backendUnloaderHandlers.Run();
        regionInvalidation.Drain();
        regionThread.Stop();
        entRegionThread.Stop();
    }
}
