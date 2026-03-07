namespace Craftdig.Dimension.Backend;

public class EntRegionState(string dir, Vector2i rloc, int levels)
{
    public readonly EntRegionFiles Files = new(dir, rloc);
    public readonly EntRegionFreeMap FreeMap = new(levels);
}
