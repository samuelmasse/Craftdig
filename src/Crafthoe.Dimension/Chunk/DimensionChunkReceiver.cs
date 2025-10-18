namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkReceiver(
    DimensionChunkThreadOutputBag outputBag,
    DimensionChunks chunks,
    DimensionChunkPending chunkPending,
    DimensionChunkBag chunkBag,
    DimensionChunkRenderScheduler chunkRenderScheduler,
    DimensionRegionWriter regionWriter)
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

        for (int sz = 0; sz < SectionHeight; sz++)
            regionWriter.Write(blocks.Span.Slice(sz * SectionVolume, SectionVolume), new(cloc, sz));

        chunks.Alloc(cloc);
        var chunk = chunks[cloc];
        chunk.Blocks(output.Blocks);

        chunkRenderScheduler.Add(cloc);
        chunkBag.Add(chunk);

        chunkPending.Remove(cloc);
    }
}
