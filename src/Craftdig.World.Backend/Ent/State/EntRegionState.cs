namespace Craftdig.World.Backend;

public class EntRegionState(string dir, Vector2i rloc, int levels)
{
    public readonly Vector2i Rloc = rloc;
    public readonly EntRegionFiles Files = new(dir);
    public readonly EntRegionFreeMap FreeMap = new(levels);
    public readonly HashSet<EntMutIdx> Ents = [];
}
