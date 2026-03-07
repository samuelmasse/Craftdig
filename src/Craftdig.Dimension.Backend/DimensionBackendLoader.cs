namespace Craftdig.Dimension.Backend;

[DimensionLoader]
public class DimensionBackendLoader(
    DimensionSavedComponentsLoader savedComponentsLoader,
    DimensionChunkThreads chunkThreads,
    DimensionRegionThread regionThread)
{
    public void Run()
    {
        savedComponentsLoader.Run();
        chunkThreads.Start();
        regionThread.Start();
    }
}
