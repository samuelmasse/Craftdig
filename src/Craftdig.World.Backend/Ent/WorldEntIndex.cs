namespace Craftdig.World.Backend;

[World]
public class WorldEntIndex
{
    private readonly Dictionary<Guid, EntMutIdx> dict = [];

    public EntMutIdx this[Guid id] => dict[id];

    public bool TryGet(Guid id, out EntMutIdx ent) => dict.TryGetValue(id, out ent);

    public void Intercept(EntMutIdx ent, in Guid value)
    {
        if (ent.Id == value)
            return;

        if (ent.Id != default)
            dict.Remove(ent.Id);

        if (value != default)
            dict.Add(value, ent);
    }

    public void InterceptDispose(EntMutIdx ent)
    {
        if (ent.Id != default)
            dict.Remove(ent.Id);
    }
}
