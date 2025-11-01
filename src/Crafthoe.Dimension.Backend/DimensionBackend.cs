namespace Crafthoe.Dimension.Backend;

[Dimension]
public class DimensionBackend(
    DimensionRigids rigids,
    DimensionMovement movement,
    DimensionChunkRequester chunkRequester,
    DimensionChunkCollector chunkCollector,
    DimensionChunkReceiver chunkReceiver,
    DimensionRegionReceiver regionReceiver,
    DimensionRegionInvalidation regionInvalidation)
{
    public void Tick()
    {
        movement.Tick();
        rigids.Tick();
    }

    public void Frame()
    {
        regionInvalidation.Frame();
        chunkCollector.Frame();
        chunkRequester.Frame();
        chunkReceiver.Frame();
        regionReceiver.Frame();
    }
}
