namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionBackend(
    DimensionEntTracker entTracker,
    DimensionEntPersister entPersister,
    DimensionChunkRequester chunkRequester,
    DimensionChunkReceiver chunkReceiver,
    DimensionRegionReceiver regionReceiver,
    DimensionRegionInvalidation regionInvalidation)
{
    public void Tick()
    {
        entTracker.Tick();
    }

    public void Frame()
    {
        entPersister.Frame();
        regionInvalidation.Frame();
        chunkRequester.Frame();
        chunkReceiver.Frame();
        regionReceiver.Frame();
    }
}
