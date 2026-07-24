namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionPlayerIndex
{
    private readonly Dictionary<Guid, EntMutIdx> dict = [];
    private readonly Dictionary<EntMutIdx, Guid> ids = [];
    private readonly HashSet<Guid> collisions = [];

    public EntMutIdx this[Guid id] => TryGet(id, out var ent) ? ent :
        throw new InvalidDataException($"Dimension player ID {id} is missing or duplicated.");

    public bool TryGet(Guid id, out EntMutIdx ent)
    {
        if (collisions.Contains(id))
        {
            ent = default;
            return false;
        }

        return dict.TryGetValue(id, out ent);
    }

    public bool IsDuplicated(Guid id) => collisions.Contains(id);

    public void Intercept(EntMutIdx ent)
    {
        Remove(ent);
        if (!ent.IsPlayer)
            return;

        Guid id = ent.WorldPlayer != default ? ent.WorldPlayer.Id : ent.Id;
        if (id == default)
            return;

        if (!dict.TryAdd(id, ent) && dict[id] != ent)
            collisions.Add(id);
        ids[ent] = id;
    }

    public void InterceptDispose(EntMutIdx ent)
    {
        Remove(ent);
    }

    private void Remove(EntMutIdx ent)
    {
        if (!ids.Remove(ent, out Guid id))
            return;

        if (dict.TryGetValue(id, out var existing) && existing == ent)
            dict.Remove(id);

    }
}
