namespace Crafthoe.Dimension;

[Dimension]
public class DimensionServerContext(
    DimensionChunkRequester chunkRequester,
    DimensionChunkCollector chunkCollector,
    DimensionChunkReceiver chunkReceiver,
    DimensionRegionReceiver regionReceiver,
    DimensionRegionInvalidation regionInvalidation)
{
    public void Frame()
    {
        regionInvalidation.Frame();
        chunkCollector.Frame();
        chunkRequester.Frame();
        chunkReceiver.Frame();
        regionReceiver.Frame();
    }
}
