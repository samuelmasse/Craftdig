namespace Crafthoe.Dimension;

[Dimension]
public class DimensionSectionThreadWorker(
    DimensionSectionThreadBufferBag bag,
    DimensionSectionThreadOutputQueue outputQueue,
    DimensionSectionMesher mesher)
{
    public void Work(Vector3i sloc)
    {
        var buffer = bag.Take();
        mesher.Render(buffer, sloc);
        outputQueue.Enqeue(new(buffer, sloc));
    }
}
