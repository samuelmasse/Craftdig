namespace Craftdig;

[World]
public class WorldEnts
{
    private readonly HashSet<EntMutIdx> ents = [];

    public HashSet<EntMutIdx>.Enumerator GetEnumerator() => ents.GetEnumerator();

    public void Remove(EntMutIdx ent) => ents.Remove(ent);

    public void Update(EntMutIdx ent, in EntChange<Guid> change)
    {
        if (change.IsPresent)
            ents.Add(ent);
        else ents.Remove(ent);
    }
}
