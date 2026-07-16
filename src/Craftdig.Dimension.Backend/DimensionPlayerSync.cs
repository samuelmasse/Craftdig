namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionPlayerSync(DimensionEnt dimension)
{
    public void Intercept(EntMutIdx ent)
    {
        if (ent.IsLoading)
            return;

        if (!ent.IsPlayer)
            return;

        ent.WorldPlayer.Mutate()
            .WorldPosition(ent.Position)
            .WorldDimension(dimension);
    }
}
