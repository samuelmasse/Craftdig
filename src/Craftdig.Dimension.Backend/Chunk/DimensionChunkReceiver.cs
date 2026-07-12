namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionChunkReceiver(
    DimensionChunkThreadOutputBag outputBag,
    DimensionChunks chunks,
    DimensionChunkPending chunkPending,
    DimensionRegionThreadWorkQueue regionThreadWorkQueue,
    DimensionChunkReceiverHandlers chunkReceiverHandlers,
    DimensionLighting lighting)
{
    private readonly Stopwatch watch = new();

    public void Frame()
    {
        int count = outputBag.Count;
        int received = 0;
        watch.Restart();

        while (count > 0 &&
               (received == 0 || watch.Elapsed.TotalMilliseconds < 2) &&
               outputBag.TryTake(out var output))
        {
            Receive(output);
            count--;
            received++;
        }
    }

    private void Receive(ChunkThreadOutput output)
    {
        var cloc = output.Cloc;

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
        chunk.IsLoaded = true;
        lighting.Load(chunk, output.Light);
        chunkPending.Remove(cloc);
        chunkReceiverHandlers.Run(chunk);
    }
}
