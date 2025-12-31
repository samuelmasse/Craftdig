namespace Craftdig.Dimension.Backend;

public class RegionState(string dir, Vector2i rloc, int levels)
{
    public Vector3i Origin = new(rloc.X << RegionBits, rloc.Y << RegionBits, 0);
    public readonly RegionFiles Files = new(dir, rloc);
    public readonly RegionFreeMap FreeMap = new(levels);
    public readonly RegionIndex Index = new();
}
