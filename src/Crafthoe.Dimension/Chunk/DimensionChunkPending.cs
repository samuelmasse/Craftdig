namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkPending
{
    private readonly HashSet<Vector2i> set = [];

    public void Add(Vector2i cloc) => set.Add(cloc);
    public void Remove(Vector2i cloc) => set.Remove(cloc);
    public bool Contains(Vector2i cloc) => set.Contains(cloc);
}
