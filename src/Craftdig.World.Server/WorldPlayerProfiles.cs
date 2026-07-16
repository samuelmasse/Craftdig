namespace Craftdig.World.Server;

[World]
public class WorldPlayerProfiles(
    WorldEntArena entArena,
    WorldEntIndex entIndex)
{
    public EntMutIdx GetOrCreate(Guid id, out bool created)
    {
        if (entIndex.TryGet(id, out var ent))
        {
            created = false;
            return ent;
        }

        created = true;
        return entArena.Alloc(id).Mutate()
            .IsWorldPlayer(true)
            .Ent;
    }
}
