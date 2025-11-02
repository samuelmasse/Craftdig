namespace Crafthoe.Dimension;

[Dimension]
public class DimensionContext(
    DimensionScope scope,
    DimensionBlockChanges blockChanges,
    DimensionChunkCollector chunkCollector)
{
    public DimensionScope Scope => scope;

    public void Frame()
    {
        blockChanges.Clear();
        chunkCollector.Frame();
    }
}
