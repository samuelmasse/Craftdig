namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRegionReceiver(
    DimensionRegionThreadOutputBag outputBag,
    DimensionChunkThreadWorkQueue chunkThreadWorkQueue)
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

    private void Receive(RegionThreadOutput output)
    {
        if (output.Input.Type == RegionThreadInputType.ReadChunk)
            chunkThreadWorkQueue.Enqeue(new(output.Input.Blocks, output.Input.Sloc.Xy, output.Noop));
    }
}
