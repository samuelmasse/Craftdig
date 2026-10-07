namespace Craftdig;

[WorldLoader]
public class WorldLoader(
    WorldEntIdxContext context,
    WorldEnts ents,
    WorldDimensionBag dimensionBag)
{
    public void Run()
    {
        context.AddIndex(ents.Remove).OnChange<Guid, WorldComponents.Id>(ents.Update);
        context.AddGatedBag(dimensionBag);
    }
}
