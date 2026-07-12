namespace Craftdig.Dimension;

[Dimension]
public class DimensionContext(
    DimensionRigids rigids,
    DimensionMovement movement,
    DimensionBlockChanges blockChanges,
    DimensionLightChanges lightChanges,
    DimensionLighting lighting,
    DimensionChunkCollector chunkCollector,
    DimensionSelected selected,
    DimensionConstruction construction)
{
    public void Tick()
    {
        selected.Tick();
        construction.Tick();
        lighting.Tick();
        movement.Tick();
        rigids.Tick();
    }

    public void Frame()
    {
        blockChanges.Clear();
        lightChanges.Clear();
        chunkCollector.Frame();
    }
}
