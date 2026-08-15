namespace Craftdig;

[WorldLoader]
public class WorldLoader(
    WorldEntIdxContextBuilder context,
    WorldEnts ents,
    WorldDimensionBagMut dimensionBag)
{
    public void Run()
    {
        context.AddPost<Guid, WorldComponents.Id>(ents.Intercept);
        context.AddGatedBag(dimensionBag);
    }
}
