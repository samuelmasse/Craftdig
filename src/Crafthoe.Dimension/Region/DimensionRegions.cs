namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRegions(WorldEntPtrBag entPtrBag)
{
    private readonly L3Map512<EntPtr> map = new();

    public EntMut this[Vector2i rloc]
    {
        get
        {
            if (TryGet(rloc, out var r))
                return r;

            throw new KeyNotFoundException();
        }
    }

    public bool TryGet(Vector2i rloc, out EntMut region)
    {
        if (map.TryGet(rloc, out var ptr))
        {
            region = ptr;
            return region != default;
        }
        else
        {
            region = default;
            return false;
        }
    }

    public bool Contains(Vector2i rloc) => TryGet(rloc, out _);

    public void Alloc(Vector2i rloc)
    {
        if (Contains(rloc))
            return;

        ref var region = ref map[rloc];
        region = new EntPtr()
            .IsRegion(true)
            .Rloc(rloc);
        entPtrBag.Add(region);
    }

    public void Free(Vector2i rloc)
    {
        if (!Contains(rloc))
            return;

        ref var region = ref map[rloc];
        entPtrBag.Remove(region);
        region.Dispose();
        region = default;
    }
}
