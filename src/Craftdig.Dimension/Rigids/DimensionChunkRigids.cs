namespace Craftdig;

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

    public void UpdatePosition(EntMutIdx ent, in EntChange<Vec3d> change)
    {
        Vec2i? previous = ent.IsRigid ? change.Before.ToLoc().Xy.ToCloc() : null;
        Vec2i? current = ent.IsRigid ? change.After.ToLoc().Xy.ToCloc() : null;
        Move(ent, previous, current);
    }

    public void UpdateRigid(EntMutIdx ent, in EntChange<bool> change)
    {
        var cloc = ent.Position.ToLoc().Xy.ToCloc();
        Move(ent, change.Before ? cloc : null, change.After ? cloc : null);
    }

    public void Remove(EntMutIdx ent)
    {
        if (ent.IsRigid)
            Remove(ent, ent.Position.ToLoc().Xy.ToCloc());
    }

    private void Move(EntMutIdx ent, Vec2i? previous, Vec2i? current)
    {
        if (previous == current)
            return;

        if (previous != null)
            Remove(ent, previous.Value);

        if (current != null)
        {
            if (!dict.TryGetValue(current.Value, out var set))
            {
                set = [];
                dict.Add(current.Value, set);
            }

            set.Add(ent);
        }
    }

    private void Remove(EntMutIdx ent, Vec2i cloc)
    {
        var set = dict[cloc];
        set.Remove(ent);

        if (set.Count == 0)
            dict.Remove(cloc);
    }
}
