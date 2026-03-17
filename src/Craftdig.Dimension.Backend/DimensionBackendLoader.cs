namespace Craftdig.Dimension.Backend;

[DimensionLoader]
public class DimensionBackendLoader(
    DimensionSavedComponentsLoader savedComponentsLoader,
    DimensionChunkThreads chunkThreads,
    DimensionRegionThread regionThread,
    DimensionEntRegionThread entRegionThread,
    DimensionEntTracker entTracker)
{
    public void Run()
    {
        savedComponentsLoader.Run();
        chunkThreads.Start();
        regionThread.Start();
        entRegionThread.Start();
        entTracker.Tick();
    }
}
