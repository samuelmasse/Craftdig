namespace Craftdig.Dimension;

[Dimension]
public class DimensionChunkRigids
{
    private readonly HashSet<EntMutIdx> empty = [];
    private readonly Dictionary<Vec2i, HashSet<EntMutIdx>> dict = [];

    public HashSet<EntMutIdx> this[Vec2i index]
    {
        get
        {
            if (dict.TryGetValue(index, out var value))
                return value;
            else return empty;
        }
    }

    public void Intercept(EntMutIdx ent)
    {
        Vec2i? cloc = ent.IsRigid ? ent.Position.ToLoc().XY.ToCloc() : null;
        var prevCloc = ent.RigidCloc;

        if (prevCloc == cloc)
            return;

        if (prevCloc != null)
        {
            var set = dict[prevCloc.Value];
            set.Remove(ent);
            if (set.Count == 0)
                dict.Remove(prevCloc.Value);

            ent.RigidCloc = null;
        }

        if (cloc != null)
        {
            if (!dict.TryGetValue(cloc.Value, out var set))
            {
                set = [];
                dict.Add(cloc.Value, set);
            }

            set.Add(ent);
            ent.RigidCloc = cloc;
        }
    }
}
