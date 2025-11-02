namespace Crafthoe.Dimension.Frontend;

[Dimension]
public class DimensionChunkFrontendReceiver(
    DimensionChunkRenderScheduler chunkRenderScheduler,
    DimensionChunkSortedLists chunkSortedLists)
{
    public void Receive(EntMut chunk)
    {
        chunk.Unrendered() = chunkSortedLists.Take();
        chunk.Rendered() = chunkSortedLists.Take();
        chunkRenderScheduler.Add(chunk.Cloc());
    }
}
