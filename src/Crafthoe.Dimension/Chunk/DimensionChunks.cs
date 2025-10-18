namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunks(WorldEntPtrBag entPtrBag)
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
        if (map.TryGet(cloc, out var ptr))
        {
            chunk = ptr;
            return chunk != default;
        }
        else
        {
            chunk = default;
            return false;
        }
    }

    public bool Contains(Vector2i cloc) => TryGet(cloc, out _);

    public void Alloc(Vector2i cloc)
    {
        if (Contains(cloc))
            return;

        ref var chunk = ref map[cloc];
        chunk = new EntPtr()
            .IsChunk(true)
            .Cloc(cloc);
        entPtrBag.Add(chunk);
    }

    public void Free(Vector2i cloc)
    {
        if (!Contains(cloc))
            return;

        ref var chunk = ref map[cloc];
        entPtrBag.Remove(chunk);
        chunk.Dispose();
        chunk = default;
    }
}
