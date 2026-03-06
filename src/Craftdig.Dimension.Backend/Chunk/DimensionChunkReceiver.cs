namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionChunkReceiver(
    DimensionChunkThreadOutputBag outputBag,
    DimensionChunks chunks,
    DimensionChunkPending chunkPending,
    DimensionRegionThreadWorkQueue regionThreadWorkQueue,
    DimensionChunkReceiverHandlers chunkReceiverHandlers)
{
    public void Frame()
    {
        int count = outputBag.Count;

        while (count > 0 && outputBag.TryTake(out var output))
        {
            Receive(output);
            count--;
        }
    }

    private void Receive(ChunkThreadInput output)
    {
        var cloc = output.Cloc;
        var blocks = output.Blocks;

        if (!output.Noop)
        {
            for (int sz = 0; sz < SectionHeight; sz++)
            {
                regionThreadWorkQueue.Enqeue(
                    new(new(cloc, sz),
                    RegionThreadInputType.WriteSection,
                    output.Blocks,
                    sz));
            }
        }

        chunks.Alloc(cloc);
        var chunk = chunks[cloc];
        chunk.ChunkBlocks = output.Blocks;
        chunk.IsChunkLoaded = true;
        chunkPending.Remove(cloc);
        chunkReceiverHandlers.Run(chunk);
    }
}
