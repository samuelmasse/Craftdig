namespace Crafthoe.Frontend;

[Player]
public class PlayerUnloadWorldAction(WorldScope worldScope, WorldDimensionBag dimensionBag)
{
    public void Run()
    {
        foreach (var dimension in dimensionBag.Ents)
        {
            var dimensionLoaderScope = dimension.DimensionScope().Scope<DimensionLoaderScope>();
            dimensionLoaderScope.Get<DimensionClientUnloader>().Run();
            dimensionLoaderScope.Get<DimensionServerUnloader>().Run();
            dimensionLoaderScope.Get<DimensionUnloader>().Run();
            dimension.Dispose();
        }

        worldScope.Scope<WorldLoaderScope>().Get<WorldUnloader>().Run();
    }
}
