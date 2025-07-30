namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkGeneratedEvent
{
    private readonly List<Vector2i> events = [];

    public ReadOnlySpan<Vector2i> Events => CollectionsMarshal.AsSpan(events);
    public void Add(Vector2i cloc) => events.Add(cloc);
    public void Reset() => events.Clear();
}
