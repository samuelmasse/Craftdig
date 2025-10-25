namespace Crafthoe.Dimension;

[DimensionLoader]
public class DimensionClientLoader(
    DimensionSectionThreads sectionThreads,
    DimensionChunkReceiverHandlers chunkReceiverHandlers,
    DimensionClientChunkReceiverHandler clientChunkReceiverHandler,
    DimensionChunkUnloaderHandlers chunkUnloaderHandlers,
    DimensionClientChunkUnloaderHandler clientChunkUnloaderHandler)
{
    public void Run()
    {
        sectionThreads.Start();
        chunkReceiverHandlers.Add(clientChunkReceiverHandler.Handle);
        chunkUnloaderHandlers.Add(clientChunkUnloaderHandler.Handler);
    }
}
