namespace Crafthoe.Dimension;

[Dimension]
public class DimensionContext(
    DimensionChunkRequester chunkRequester,
    DimensionSectionRequester sectionRequester,
    DimensionChunkCollector chunkCollector)
{
    private bool requesterType;

    public void Update(double time)
    {

    }

    public void Tick()
    {
        chunkCollector.Collect();

        if (requesterType)
            chunkRequester.Tick();
        else sectionRequester.Tick();

        requesterType = !requesterType;
    }
}
