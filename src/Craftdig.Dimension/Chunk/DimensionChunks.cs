namespace Craftdig.Dimension;

[Dimension]
public class DimensionChunks(DimensionChunkArena chunkArena)
{
    private readonly L3Map512<EntPtrIdx> map = new();

    public EntMutIdx this[Vec2i cloc]
    {
        get
        {
            if (TryGet(cloc, out var c))
                return c;

            throw new KeyNotFoundException();
        }
    }

    public bool TryGet(Vec2i cloc, out EntMutIdx chunk)
    {
        bool res = map.TryGetValue(cloc, out var val);
        chunk = val;
        return res;
    }

    public bool Contains(Vec2i cloc) => map.ContainsKey(cloc);

    public void Alloc(Vec2i cloc)
    {
        if (Contains(cloc))
            return;

        var chunk = chunkArena.Alloc()
            .Mutate()
            .IsChunk(true)
            .Cloc(cloc)
            .Ent;

        map.Add(cloc, chunk);
    }

    public void Free(Vec2i cloc)
    {
        if (!Contains(cloc))
            return;

        var chunk = map[cloc];
        chunk.Dispose();
        map.Remove(cloc);
    }
}
