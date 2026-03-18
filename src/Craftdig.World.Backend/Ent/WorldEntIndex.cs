namespace Craftdig.World.Backend;

[World]
public class WorldEntIndex
{
    private readonly Dictionary<Guid, EntMutIdx> dict = [];

    public EntMutIdx this[Guid id] => dict[id];

    public void Intercept(EntMutIdx ent, Guid value)
    {
        if (ent.Id == value)
            return;

        if (ent.Id != default)
            dict.Remove(ent.Id);

        if (value != default)
            dict.Add(value, ent);
    }
}
