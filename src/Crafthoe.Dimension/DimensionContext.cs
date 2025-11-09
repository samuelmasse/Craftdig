namespace Crafthoe.Dimension;

[Dimension]
public class DimensionContext(
    DimensionRigids rigids,
    DimensionMovement movement,
    DimensionBlockChanges blockChanges,
    DimensionChunkCollector chunkCollector)
{
    public void Tick()
    {
        movement.Tick();
        rigids.Tick();
    }

    public void Frame()
    {
        blockChanges.Clear();
        chunkCollector.Frame();
    }
}
