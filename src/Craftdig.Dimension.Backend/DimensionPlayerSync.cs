namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionPlayerSync(WorldEntArena worldEntArena, DimensionEnt dimension)
{
    public void Intercept(EntMutIdx ent)
    {
        if (ent.IsLoading)
            return;

        if (!ent.IsPlayer)
            return;

        if (ent.WorldPlayer == default)
            ent.WorldPlayer = worldEntArena.Alloc().Mutate().IsWorldPlayer(true).Ent;

        ent.WorldPlayer.Mutate()
            .WorldPosition(ent.Position)
            .WorldDimension(dimension);
    }
}
