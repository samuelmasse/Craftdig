namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionSectionThreadWorker(
    DimensionSectionThreadBufferBag bag,
    DimensionSectionThreadOutputBag outputQueue,
    DimensionSectionMesher mesher)
{
    public void Work(Vec3i sloc)
    {
        var buffer = bag.Take();
        mesher.Render(buffer, sloc);
        outputQueue.Add(new(buffer, sloc));
    }
}
