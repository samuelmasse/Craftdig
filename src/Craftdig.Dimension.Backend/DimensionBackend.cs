namespace Craftdig;

[Dimension]
public class DimensionBackend(
    DimensionEntTracker entTracker,
    DimensionEntPersister entPersister,
    DimensionChunkRequester chunkRequester,
    DimensionChunkReceiver chunkReceiver,
    DimensionRegionReceiver regionReceiver,
    DimensionRegionInvalidation regionInvalidation,
    DimensionDropBackend drop)
{
    public void Tick()
    {
        entTracker.Tick();
        drop.Tick();
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
