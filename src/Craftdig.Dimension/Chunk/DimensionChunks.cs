namespace Craftdig.Dimension;

[Dimension]
public class DimensionChunks(DimensionEntArena entArena)
{
    private readonly L3Map512<EntPtrIdx> map = new();

    public EntMutIdx this[Vector2i cloc]
    {
        get
        {
            if (TryGet(cloc, out var c))
                return c;

            throw new KeyNotFoundException();
        }
    }

    public bool TryGet(Vector2i cloc, out EntMutIdx chunk)
    {
        bool res = map.TryGetValue(cloc, out var val);
        chunk = val;
        return res;
    }

    public bool Contains(Vector2i cloc) => map.ContainsKey(cloc);

    public void Alloc(Vector2i cloc)
    {
        if (Contains(cloc))
            return;

        var chunk = entArena.Alloc()
            .Mutate()
            .IsChunk(true)
            .Cloc(cloc)
            .Ent;

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
