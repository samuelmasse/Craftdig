namespace Crafthoe.Dimension.Backend;

[Dimension]
public class DimensionRegionThreadWorker(
    DimensionRegionThreadStates states,
    DimensionRegionWriter writer,
    DimensionRegionThreadChunkReader reader,
    DimensionBlocksPool blocksPool,
    DimensionRegionThreadFileHandles handles,
    DimensionRegionThreadOutputBag output)
{
    public void Work(RegionThreadInput input)
    {
        if (input.Type == RegionThreadInputType.Flush)
        {
            handles.Flush();
            states.Drain();
        }
        else if (input.Type == RegionThreadInputType.WriteSection)
            writer.Write(input.Blocks.Span, input.Sloc);
        else if (input.Type == RegionThreadInputType.ReadChunk)
        {
            bool noop = reader.TryRead(input.Blocks.Span, input.Sloc.Xy);
            output.Add(new(input, noop));
        }
        else if (input.Type == RegionThreadInputType.DisposeChunk)
        {
            input.Blocks.Span.Clear();
            blocksPool.Add(input.Blocks);
        }
    }
}
