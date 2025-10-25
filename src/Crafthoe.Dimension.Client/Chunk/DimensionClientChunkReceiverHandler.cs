namespace Crafthoe.Dimension;

[Dimension]
public class DimensionClientChunkReceiverHandler(
    DimensionChunkRenderScheduler chunkRenderScheduler,
    DimensionChunkSortedLists chunkSortedLists)
{
    public void Handle(EntMut chunk)
    {
        chunk.Unrendered() = chunkSortedLists.Take();
        chunk.Rendered() = chunkSortedLists.Take();
        chunkRenderScheduler.Add(chunk.Cloc());
    }
}
