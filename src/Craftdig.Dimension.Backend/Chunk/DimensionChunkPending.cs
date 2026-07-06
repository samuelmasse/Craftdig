namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionChunkPending
{
    private readonly HashSet<Vec2i> set = [];

    public void Add(Vec2i cloc) => set.Add(cloc);
    public void Remove(Vec2i cloc) => set.Remove(cloc);
    public bool Contains(Vec2i cloc) => set.Contains(cloc);
}
