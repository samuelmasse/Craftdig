namespace Craftdig.Dimension.Backend;

public class EntityRegionState(string dir, Vector2i rloc, int levels)
{
    public readonly EntityRegionFiles Files = new(dir, rloc);
    public readonly EntityRegionFreeMap FreeMap = new(levels);
}
