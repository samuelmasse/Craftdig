namespace Craftdig.World;

[WorldLoader]
public class WorldLoader(
    WorldEntIdxContextBuilder context,
    WorldDimensionBagMut dimensionBag)
{
    public void Run() => context.AddGatedBag(dimensionBag);
}
