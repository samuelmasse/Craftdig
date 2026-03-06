namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionChunkFrontendReceiver(
    DimensionChunkRenderScheduler chunkRenderScheduler,
    DimensionChunkSortedLists chunkSortedLists)
{
    public void Receive(EntMutIdx chunk)
    {
        chunk.Unrendered = chunkSortedLists.Take();
        chunk.Rendered = chunkSortedLists.Take();
        chunkRenderScheduler.Add(chunk.Cloc);
    }
}
