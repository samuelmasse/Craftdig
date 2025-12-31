namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionBackend(
    DimensionChunkRequester chunkRequester,
    DimensionChunkReceiver chunkReceiver,
    DimensionRegionReceiver regionReceiver,
    DimensionRegionInvalidation regionInvalidation)
{
    public void Frame()
    {
        regionInvalidation.Frame();
        chunkRequester.Frame();
        chunkReceiver.Frame();
        regionReceiver.Frame();
    }
}
