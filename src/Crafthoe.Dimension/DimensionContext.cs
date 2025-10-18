namespace Crafthoe.Dimension;

[Dimension]
public class DimensionContext(
    DimensionScope scope,
    DimensionChunkRequester chunkRequester,
    DimensionChunkCollector chunkCollector,
    DimensionSectionRequester sectionRequester,
    DimensionSectionReceiver sectionReceiver,
    DimensionChunkReceiver chunkReceiver,
    DimensionRegionReceiver regionReceiver,
    DimensionSectionInvalidation sectionInvalidation,
    DimensionRegionInvalidation regionInvalidation,
    DimensionBlockChanges blockChanges,
    DimensionRigids rigids)
{
    private bool requesterType;

    public DimensionScope Scope => scope;

    public void Tick()
    {
        rigids.Tick();
    }

    public void Frame()
    {
        regionInvalidation.Frame();
        sectionInvalidation.Frame();
        chunkCollector.Frame();

        if (requesterType)
            chunkRequester.Frame();
        else sectionRequester.Frame();

        chunkReceiver.Frame();
        sectionReceiver.Frame();
        regionReceiver.Frame();

        requesterType = !requesterType;

        blockChanges.Clear();
    }
}
