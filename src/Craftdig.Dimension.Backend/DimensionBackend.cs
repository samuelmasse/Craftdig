namespace Craftdig;

[Dimension]
public class DimensionBackend(
    DimensionEntPersister entPersister,
    DimensionChunkRequester chunkRequester,
    DimensionChunkReceiver chunkReceiver,
    DimensionRegionReceiver regionReceiver,
    DimensionRegionInvalidation regionInvalidation,
    DimensionDropBackend drop)
{
    public void Tick()
    {
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
