namespace Crafthoe.Dimension;

[Dimension]
public class DimensionContext(
    DimensionScope scope,
    DimensionChunkRequester chunkRequester,
    DimensionChunkCollector chunkCollector,
    DimensionSectionRequester sectionRequester,
    DimensionSectionInvalidation sectionInvalidation,
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
        sectionInvalidation.Frame();
        chunkCollector.Frame();

        if (requesterType)
            chunkRequester.Frame();
        else sectionRequester.Frame();

        requesterType = !requesterType;

        blockChanges.Clear();
    }
}
