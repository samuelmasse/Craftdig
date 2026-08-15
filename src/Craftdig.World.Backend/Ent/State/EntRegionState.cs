namespace Craftdig;

public class EntRegionState(string dir, Vec2i rloc, int levels)
{
    public readonly Vec2i Rloc = rloc;
    public readonly EntRegionFiles Files = new(dir);
    public readonly EntRegionFreeMap FreeMap = new(levels);
    public readonly HashSet<EntMutIdx> Ents = [];
}
