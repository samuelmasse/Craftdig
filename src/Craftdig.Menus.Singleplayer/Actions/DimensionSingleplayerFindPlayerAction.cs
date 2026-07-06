namespace Craftdig.Menus.Singleplayer;

[Dimension]
public class DimensionSingleplayerFindPlayerAction(
    WorldEntRegionStates worldEntRegionStates,
    DimensionEntRegionStates dimensionEntRegionStates,
    DimensionEntArena dimensionEntArena)
{
    public EntMutIdx Run()
    {
        var worldEntRegion = worldEntRegionStates[default];
        var existingPlayer = worldEntRegion.Ents.FirstOrDefault(x => x.IsWorldPlayer);

        if (existingPlayer != default)
        {
            var rloc = existingPlayer.WorldPosition.ToLoc().XY.ToCloc().ToRloc();
            var dimensionEntRegion = dimensionEntRegionStates[rloc];
            return dimensionEntRegion.Ents.First(x => x.WorldPlayer == existingPlayer);
        }

        return dimensionEntArena.Alloc();
    }
}
