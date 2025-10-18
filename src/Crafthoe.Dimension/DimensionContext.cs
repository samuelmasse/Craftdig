namespace Crafthoe.Dimension;

[Dimension]
public class DimensionContext(
    DimensionScope scope,
    DimensionChunkRequester chunkRequester,
    DimensionChunkCollector chunkCollector,
    DimensionSectionRequester sectionRequester,
    DimensionSectionReceiver sectionReceiver,
    DimensionChunkReceiver chunkReceiver,
    DimensionSectionInvalidation sectionInvalidation,
    DimensionRegionInvalidation regionInvalidation,
    DimensionBlockChanges blockChanges,
    DimensionRegionFileHandles regionFileHandles,
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

        requesterType = !requesterType;

        blockChanges.Clear();
        regionFileHandles.Flush();
    }
}
