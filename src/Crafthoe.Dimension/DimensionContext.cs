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

    public void Update(double time)
    {
        rigids.Tick();
    }

    public void Tick()
    {
        sectionInvalidation.Tick();
        chunkCollector.Collect();

        if (requesterType)
            chunkRequester.Tick();
        else sectionRequester.Tick();

        requesterType = !requesterType;

        blockChanges.Clear();
    }
}
