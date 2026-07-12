namespace Craftdig.Dimension;

[Dimension]
public class DimensionLightChanges
{
    private readonly HashSet<Vec3i> sections = [];

    public int Count => sections.Count;

    public HashSet<Vec3i>.Enumerator GetEnumerator() => sections.GetEnumerator();

    public void Add(Vec3i loc)
    {
        int minX = (loc.X & SectionMask) == 0 ? -1 : 0;
        int maxX = (loc.X & SectionMask) == SectionMask ? 1 : 0;
        int minY = (loc.Y & SectionMask) == 0 ? -1 : 0;
        int maxY = (loc.Y & SectionMask) == SectionMask ? 1 : 0;
        int minZ = (loc.Z & SectionMask) == 0 ? -1 : 0;
        int maxZ = (loc.Z & SectionMask) == SectionMask ? 1 : 0;

        for (int dz = minZ; dz <= maxZ; dz++)
        {
            for (int dy = minY; dy <= maxY; dy++)
            {
                for (int dx = minX; dx <= maxX; dx++)
                {
                    var sloc = (loc + (dx, dy, dz)).ToSloc();
                    if ((uint)sloc.Z < SectionHeight)
                        sections.Add(sloc);
                }
            }
        }
    }

    public void AddSection(Vec3i sloc)
    {
        if ((uint)sloc.Z < SectionHeight)
            sections.Add(sloc);
    }

    public void Clear() => sections.Clear();
}
