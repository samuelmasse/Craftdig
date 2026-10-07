namespace Craftdig;

[Dimension]
public class DimensionPlayerIndex
{
    private readonly Dictionary<Guid, EntMutIdx> dict = [];
    private readonly Dictionary<Guid, List<EntMutIdx>> collisions = [];

    public EntMutIdx this[Guid id] => TryGet(id, out var ent) ? ent :
        throw new InvalidDataException($"Dimension player ID {id} is missing or duplicated.");

    public bool TryGet(Guid id, out EntMutIdx ent)
    {
        if (collisions.ContainsKey(id))
        {
            ent = default;
            return false;
        }

        return dict.TryGetValue(id, out ent);
    }

    public bool IsDuplicated(Guid id) => collisions.ContainsKey(id);

    public void UpdateId(EntMutIdx ent, in EntChange<Guid> change)
    {
        if (ent.IsPlayer)
            Update(ent, change.Before, change.After);
    }

    public void UpdatePlayer(EntMutIdx ent, in EntChange<bool> change) =>
        Update(ent, change.Before ? ent.Id : Guid.Empty, change.After ? ent.Id : Guid.Empty);

    public void Remove(EntMutIdx ent)
    {
        if (ent.IsPlayer && ent.Id != Guid.Empty)
            Remove(ent.Id, ent);
    }

    private void Update(EntMutIdx ent, Guid before, Guid after)
    {
        if (before == after)
            return;

        if (before != Guid.Empty)
            Remove(before, ent);

        if (after != Guid.Empty)
            Add(after, ent);
    }

    private void Add(Guid id, EntMutIdx ent)
    {
        if (dict.TryAdd(id, ent) || dict[id] == ent)
            return;

        if (!collisions.TryGetValue(id, out var entries))
        {
            entries = [dict[id]];
            collisions.Add(id, entries);
        }

        entries.Add(ent);
    }

    private void Remove(Guid id, EntMutIdx ent)
    {
        if (collisions.TryGetValue(id, out var entries))
        {
            entries.Remove(ent);

            if (entries.Count > 1)
                return;

            collisions.Remove(id);

            if (entries.Count == 1)
                dict[id] = entries[0];
            else dict.Remove(id);

            return;
        }

        if (dict.TryGetValue(id, out var existing) && existing == ent)
            dict.Remove(id);
    }
}
