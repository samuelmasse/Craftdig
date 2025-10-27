namespace Crafthoe.Dimension;

[Dimension]
public class DimensionServerContext(
    DimensionRigids rigids,
    DimensionChunkRequester chunkRequester,
    DimensionChunkCollector chunkCollector,
    DimensionChunkReceiver chunkReceiver,
    DimensionRegionReceiver regionReceiver,
    DimensionRegionInvalidation regionInvalidation)
{
    public void Tick()
    {
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
