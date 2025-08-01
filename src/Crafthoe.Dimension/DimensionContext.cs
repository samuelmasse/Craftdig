namespace Crafthoe.Dimension;

[Dimension]
public class DimensionContext(
    DimensionChunkRequester chunkRequester,
    DimensionSectionRequester sectionRequester,
    DimensionChunkGeneratedEvent chunkGeneratedEvent,
    DimensionChunkRenderScheduler chunkRenderScheduler)
{
    public void Update(double time)
    {

    }

    public void Tick()
    {
        chunkRequester.Tick();
        sectionRequester.Tick();

        chunkRenderScheduler.Tick();
        chunkGeneratedEvent.Reset();
    }
}
