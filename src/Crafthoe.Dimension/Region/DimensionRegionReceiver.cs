namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRegionReceiver(
    DimensionRegionThreadOutputBag outputBag,
    DimensionChunkThreadWorkQueue chunkThreadWorkQueue)
{
    public void Frame()
    {
        while (outputBag.TryTake(out var output))
            Receive(output);
    }

    private void Receive(RegionThreadOutput output)
    {
        if (output.Input.Type == RegionThreadInputType.ReadChunk)
            chunkThreadWorkQueue.Enqeue(new(output.Input.Blocks, output.Input.Sloc.Xy, output.Noop));
    }
}
