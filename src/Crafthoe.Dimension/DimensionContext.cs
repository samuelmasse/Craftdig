namespace Crafthoe.Dimension;

[Dimension]
public class DimensionContext(
    DimensionScope scope,
    DimensionChunkRequester chunkRequester,
    DimensionChunkCollector chunkCollector,
    DimensionSectionRequester sectionRequester,
    DimensionSectionInvalidation sectionInvalidation,
    DimensionBlockChanges blockChanges)
{
    private bool requesterType;

    public DimensionScope Scope => scope;

    public void Update(double time)
    {

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
