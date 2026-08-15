namespace Craftdig;

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
        chunk.IsLightReady = false;
        chunk.ChunkLight?.Clear();
        chunk.ChunkLight = output.Light;
        chunk.IsLoaded = true;
        lighting.ConnectChunk(cloc);
        chunk.IsLightReady = true;
        chunkPending.Remove(cloc);
        chunkReceiverHandlers.Run(chunk);
    }
}
