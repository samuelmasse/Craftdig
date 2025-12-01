namespace Crafthoe.Client;

[Player]
public class PlayerChunks(
    DimensionChunks chunks,
    DimensionChunkBag chunkBag,
    DimensionChunkFrontendReceiver chunkReceiverHandler,
    PlayerChunkUpdateQueue chunkUpdateQueue)
{
    public void Frame()
    {
        int count = chunkUpdateQueue.Count;
        while (count > 0 && chunkUpdateQueue.TryDequeue(out var item))
        {
            var (cloc, blocks) = item;

            chunks.Alloc(cloc);
            var chunk = chunks[cloc];
            chunk.ChunkBlocks() = blocks;
            chunkBag.Add(chunk);
            chunkReceiverHandler.Receive(chunk);

            count--;
        }
    }
}
