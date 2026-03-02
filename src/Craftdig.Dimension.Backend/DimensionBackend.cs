namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionBackend(
    DimensionEntitySorter entitySorter,
    DimensionEntityPersister entityPersister,
    DimensionChunkRequester chunkRequester,
    DimensionChunkReceiver chunkReceiver,
    DimensionRegionReceiver regionReceiver,
    DimensionRegionInvalidation regionInvalidation)
{
    public void Tick()
    {
        entitySorter.Tick();
    }

    public void Frame()
    {
        entityPersister.Frame();
        regionInvalidation.Frame();
        chunkRequester.Frame();
        chunkReceiver.Frame();
        regionReceiver.Frame();
    }
}
