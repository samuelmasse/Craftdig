namespace Craftdig;

[World]
public class WorldEnts
{
    private readonly HashSet<EntMutIdx> ents = [];

    public HashSet<EntMutIdx>.Enumerator GetEnumerator() => ents.GetEnumerator();

    public void Intercept(EntMutIdx ent)
    {
        if (ent.Has<Guid, WorldComponents.Id>())
            ents.Add(ent);
        else ents.Remove(ent);
    }
}
