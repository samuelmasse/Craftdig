namespace Craftdig.Dimension;

[Dimension]
public class DimensionContext(
    DimensionRigids rigids,
    DimensionRigidSorter rigidSorter,
    DimensionMovement movement,
    DimensionBlockChanges blockChanges,
    DimensionChunkCollector chunkCollector,
    DimensionSelected selected,
    DimensionConstruction construction)
{
    public void Tick()
    {
        selected.Tick();
        construction.Tick();
        movement.Tick();
        rigids.Tick();
        rigidSorter.Tick();
    }

    public void Frame()
    {
        blockChanges.Clear();
        chunkCollector.Frame();
    }
}
