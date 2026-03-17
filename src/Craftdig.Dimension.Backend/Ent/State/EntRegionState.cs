namespace Craftdig.Dimension.Backend;

public class EntRegionState(string dir, Vector2i rloc, int levels)
{
    public readonly Vector2i Rloc = rloc;
    public readonly EntRegionFiles Files = new(dir, rloc);
    public readonly EntRegionFreeMap FreeMap = new(levels);
    public readonly HashSet<EntMutIdx> Ents = [];
}
