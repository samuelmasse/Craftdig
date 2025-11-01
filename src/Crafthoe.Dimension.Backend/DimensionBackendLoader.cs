namespace Crafthoe.Dimension.Backend;

[DimensionLoader]
public class DimensionBackendLoader(
    DimensionChunkThreads chunkThreads,
    DimensionRegionThread regionThread)
{
    public void Run()
    {
        chunkThreads.Start();
        regionThread.Start();
    }
}
