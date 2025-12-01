namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunks(WorldEntArena entArena)
{
    private readonly L3Map512<EntPtr> map = new();

    public EntMut this[Vector2i cloc]
    {
        get
        {
            if (TryGet(cloc, out var c))
                return c;

            throw new KeyNotFoundException();
        }
    }

    public bool TryGet(Vector2i cloc, out EntMut chunk)
    {
        bool res = map.TryGetValue(cloc, out var val);
        chunk = (EntMut)val;
        return res;
    }

    public bool Contains(Vector2i cloc) => map.ContainsKey(cloc);

    public void Alloc(Vector2i cloc)
    {
        if (Contains(cloc))
            return;

        var chunk = entArena.Alloc()
            .IsChunk(true)
            .Cloc(cloc);
        map.Add(cloc, chunk);
    }

    public void Free(Vector2i cloc)
    {
        if (!Contains(cloc))
            return;

        var chunk = map[cloc];
        chunk.Dispose();
        map.Remove(cloc);
    }
}
