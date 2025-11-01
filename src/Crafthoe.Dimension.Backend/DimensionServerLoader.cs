namespace Crafthoe.Dimension.Backend;

[DimensionLoader]
public class DimensionServerLoader(
    DimensionChunkThreads chunkThreads,
    DimensionRegionThread regionThread)
{
    public void Run()
    {
        chunkThreads.Start();
        regionThread.Start();
    }
}
