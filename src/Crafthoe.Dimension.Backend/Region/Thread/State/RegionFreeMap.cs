namespace Crafthoe.Dimension;

public class RegionFreeMap
{
    private readonly (int Next, HashSet<int> Holes)[] free;

    public RegionFreeMap(int levels)
    {
        free = new (int Next, HashSet<int> Holes)[levels];
        for (int i = 0; i < levels; i++)
            free[i].Holes = [];
    }

    public void Take(int bucket, int offset)
    {
        ref var b = ref free[bucket];
        while (b.Next <= offset)
            b.Holes.Add(b.Next++);

        b.Holes.Remove(offset);
    }

    public void Free(int bucket, int offset)
    {
        free[bucket].Holes.Add(offset);
    }

    public int Alloc(int bucket)
    {
        ref var b = ref free[bucket];

        if (b.Holes.Count > 0)
        {
            var hole = b.Holes.First();
            b.Holes.Remove(hole);
            return hole;
        }

        return b.Next++;
    }
}
