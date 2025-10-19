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
    public DimensionScope Scope => scope;

    public void Tick()
    {
        rigids.Tick();
    }

    public void Frame()
    {
        regionInvalidation.Frame();
        sectionInvalidation.Frame();

        var now = DateTime.UtcNow;
        chunkCollector.Frame();

        var dt = DateTime.UtcNow - now;
        if (dt.TotalMilliseconds > 0.5)
            Console.WriteLine($"over {dt.TotalMilliseconds}");

        chunkRequester.Frame();
        sectionRequester.Frame();

        chunkReceiver.Frame();
        sectionReceiver.Frame();
        regionReceiver.Frame();

        blockChanges.Clear();
    }
}
