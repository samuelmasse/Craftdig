namespace Craftdig;

[World]
public class WorldEntIndex
{
    private readonly Dictionary<Guid, EntMutIdx> dict = [];
    private readonly Dictionary<Guid, List<EntMutIdx>> collisions = [];

    public EntMutIdx this[Guid id] => TryGet(id, out var ent) ? ent :
        throw new InvalidDataException($"World Ent ID {id} is missing or duplicated.");

    public bool TryGet(Guid id, out EntMutIdx ent)
    {
        if (collisions.ContainsKey(id))
        {
            ent = default;
            return false;
        }

        return dict.TryGetValue(id, out ent);
    }

    public bool Contains(Guid id) => dict.ContainsKey(id);

    public bool IsDuplicated(Guid id) => collisions.ContainsKey(id);

    public void Intercept(EntMutIdx ent, in Guid value)
    {
        if (ent.Id == value)
            return;

        if (ent.Id != default)
            Remove(ent.Id, ent);

        if (value != default)
            Add(value, ent);
    }

    public void InterceptDispose(EntMutIdx ent)
    {
        if (ent.Id != default)
            Remove(ent.Id, ent);
    }

    private void Add(Guid id, EntMutIdx ent)
    {
        if (!dict.TryAdd(id, ent) && dict[id] != ent)
        {
            if (!collisions.TryGetValue(id, out var entries))
            {
                entries = [dict[id]];
                collisions.Add(id, entries);
            }

            entries.Add(ent);
        }
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
            else
                dict.Remove(id);
            return;
        }

        if (dict.TryGetValue(id, out var existing) && existing == ent)
            dict.Remove(id);
    }
}
