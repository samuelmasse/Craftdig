namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionSectionThreadWorker(
    DimensionSectionThreadBufferBag bag,
    DimensionSectionThreadOutputBag outputQueue,
    DimensionSectionMesher mesher)
{
    public void Work(SectionThreadInput input, SectionThreadSamples samples)
    {
        var buffer = bag.Take();
        mesher.Render(buffer, input.Sloc, samples);
        outputQueue.Add(new(buffer, input.Sloc, input.Revision));
    }
}
