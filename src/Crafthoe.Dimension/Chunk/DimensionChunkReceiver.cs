namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkReceiver(
    DimensionChunkThreadOutputBag outputBag,
    DimensionChunks chunks,
    DimensionChunkPending chunkPending,
    DimensionChunkBag chunkBag,
    DimensionChunkRenderScheduler chunkRenderScheduler,
    DimensionChunkSortedLists chunkSortedLists,
    DimensionRegionThreadWorkQueue regionThreadWorkQueue)
{
    public void Frame()
    {
        while (outputBag.TryTake(out var output))
            Receive(output);
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
                    blocks.Slice(sz * SectionVolume, SectionVolume)));
            }
        }

        chunks.Alloc(cloc);
        var chunk = chunks[cloc];
        chunk.Blocks() = output.Blocks;
        chunk.Unrendered() = chunkSortedLists.Take();
        chunk.Rendered() = chunkSortedLists.Take();

        chunkRenderScheduler.Add(cloc);
        chunkBag.Add(chunk);

        chunkPending.Remove(cloc);
    }
}
