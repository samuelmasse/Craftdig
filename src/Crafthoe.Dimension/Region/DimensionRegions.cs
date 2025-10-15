namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRegions(WorldEntPtrBag entPtrBag)
{
    private readonly L3Map512<EntPtr> map = new();

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

    public EntMut Get(Vector2i rloc)
    {
        if (TryGet(rloc, out var val))
            return val;

        ref var region = ref map[rloc];
        region = new EntPtr()
            .IsRegion(true)
            .Rloc(rloc);
        entPtrBag.Add(region);

        return region;
    }

    public void Free(Vector2i rloc)
    {
        if (!TryGet(rloc, out var _))
            return;

        ref var region = ref map[rloc];
        entPtrBag.Remove(region);
        region.Dispose();
        region = default;
    }
}
