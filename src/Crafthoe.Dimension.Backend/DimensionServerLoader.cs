namespace Crafthoe.Dimension;

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
