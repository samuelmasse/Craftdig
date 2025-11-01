namespace Crafthoe.Menus;

[Player]
public class PlayerSinglePlayerUnloadWorldAction(WorldScope worldScope, WorldDimensionBag dimensionBag, PlayerMetrics metrics)
{
    public void Run()
    {
        metrics.Stop();

        foreach (var dimension in dimensionBag.Ents)
        {
            var dimensionLoaderScope = dimension.DimensionScope().Scope<DimensionLoaderScope>();
            dimensionLoaderScope.Get<DimensionFrontendUnloader>().Run();
            dimensionLoaderScope.Get<DimensionBackendUnloader>().Run();
            dimensionLoaderScope.Get<DimensionUnloader>().Run();
            dimension.Dispose();
        }

        worldScope.Scope<WorldLoaderScope>().Get<WorldFrontendUnloader>().Run();
        worldScope.Scope<WorldLoaderScope>().Get<WorldUnloader>().Run();
    }
}
