namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunks(WorldEntPtrBag entPtrBag)
{
    private readonly L3Map512<EntPtr> map = new();

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

    public EntMut Get(Vector2i cloc)
    {
        if (TryGet(cloc, out var val))
            return val;

        ref var chunk = ref map[cloc];
        chunk = new EntPtr()
            .IsChunk(true)
            .Cloc(cloc);
        entPtrBag.Add(chunk);

        return chunk;
    }

    public void Free(Vector2i cloc)
    {
        if (!TryGet(cloc, out var _))
            return;

        ref var chunk = ref map[cloc];
        entPtrBag.Remove(chunk);
        chunk.Dispose();
        chunk = default;
    }
}
