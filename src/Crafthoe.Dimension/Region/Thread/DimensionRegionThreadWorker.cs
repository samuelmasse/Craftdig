namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRegionThreadWorker(
    DimensionRegionWriter regionWriter,
    DimensionRegionThreadChunkReader regionChunkReader,
    DimensionBlocksPool blocksPool,
    DimensionRegionThreadFileHandles regionFileHandles,
    DimensionRegionThreadOutputBag output)
{
    public void Work(RegionThreadInput input)
    {
        if (input.Type == RegionThreadInputType.Flush)
            regionFileHandles.Flush();
        else if (input.Type == RegionThreadInputType.WriteSection)
            regionWriter.Write(input.Blocks.Span, input.Sloc);
        else if (input.Type == RegionThreadInputType.ReadChunk)
        {
            bool noop = regionChunkReader.TryRead(input.Blocks.Span, input.Sloc.Xy);
            output.Add(new(input, noop));
        }
        else if (input.Type == RegionThreadInputType.DisposeChunk)
        {
            input.Blocks.Span.Clear();
            blocksPool.Add(input.Blocks);
        }
    }
}
