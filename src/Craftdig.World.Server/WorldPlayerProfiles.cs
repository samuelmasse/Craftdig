namespace Craftdig.World.Server;

[World]
public class WorldPlayerProfiles(
    WorldEntArena entArena,
    WorldEntIndex entIndex)
{
    public string? GetOrCreate(Guid id, out EntMutIdx profile, out bool created)
    {
        created = false;
        if (entIndex.TryGet(id, out profile))
        {
            if (!profile.IsWorldPlayer)
            {
                profile = default;
                return $"world Ent ID {id} is already used by a non-player Ent";
            }

            return null;
        }

        if (entIndex.Contains(id))
        {
            profile = default;
            return $"world player ID {id} is duplicated";
        }

        created = true;
        profile = entArena.Alloc(id).Mutate()
            .IsWorldPlayer(true)
            .Ent;
        return null;
    }

    public bool TryGet(Guid id, out EntMutIdx profile)
    {
        if (entIndex.TryGet(id, out profile) && profile.IsWorldPlayer)
            return true;

        profile = default;
        return false;
    }
}
