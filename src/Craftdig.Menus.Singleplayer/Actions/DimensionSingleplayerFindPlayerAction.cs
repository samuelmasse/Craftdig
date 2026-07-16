namespace Craftdig.Menus.Singleplayer;

[Dimension]
public class DimensionSingleplayerFindPlayerAction(
    WorldEntArena worldEntArena,
    WorldEntRegionStates worldEntRegionStates,
    DimensionEntRegionStates dimensionEntRegionStates,
    DimensionPlayerIndex playerIndex,
    DimensionEntArena dimensionEntArena)
{
    public EntMutIdx Run()
    {
        var worldEntRegion = worldEntRegionStates[default];
        var existingPlayer = worldEntRegion.Ents.FirstOrDefault(x => x.IsWorldPlayer);

        if (existingPlayer != default)
        {
            var rloc = existingPlayer.WorldPosition.ToLoc().Xy.ToCloc().ToRloc();
            dimensionEntRegionStates.EnsureLoaded(rloc);
            return playerIndex[existingPlayer.Id];
        }

        var worldPlayer = worldEntArena.Alloc().Mutate()
            .IsWorldPlayer(true)
            .Ent;
        return dimensionEntArena.Alloc().Mutate()
            .WorldPlayer(worldPlayer)
            .Ent;
    }
}
