namespace Craftdig;

[Dimension]
public class DimensionPlayerSync(
    WorldEntIndex worldEntIndex,
    DimensionEnt dimension)
{
    public void Intercept(EntMutIdx ent)
    {
        if (ent.IsLoading)
            return;

        if (!ent.IsPlayer)
            return;

        if (!TryGetProfile(ent, out var profile))
            return;

        profile.Mutate()
            .WorldPosition(ent.Position)
            .WorldDimension(dimension);
    }

    private bool TryGetProfile(EntMutIdx player, out EntMutIdx profile)
    {
        if (player.Id != default &&
            worldEntIndex.TryGet(player.Id, out profile) &&
            profile.IsWorldPlayer)
            return true;

        profile = player.WorldPlayer;
        return profile != default && profile.IsWorldPlayer;
    }
}
