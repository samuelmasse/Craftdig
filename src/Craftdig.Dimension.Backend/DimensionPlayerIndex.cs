namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionPlayerIndex
{
    private readonly Dictionary<Guid, EntMutIdx> dict = [];

    public EntMutIdx this[Guid id] => dict[id];

    public bool TryGet(Guid id, out EntMutIdx ent) => dict.TryGetValue(id, out ent);

    public void Intercept(EntMutIdx ent)
    {
        if (ent.WorldPlayer != default)
            dict.Add(ent.WorldPlayer.Id, ent);
    }

    public void InterceptDispose(EntMutIdx ent)
    {
        if (ent.WorldPlayer != default)
            dict.Remove(ent.WorldPlayer.Id);
    }
}
