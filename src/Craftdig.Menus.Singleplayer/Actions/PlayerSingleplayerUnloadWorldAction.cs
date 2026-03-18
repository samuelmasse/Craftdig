namespace Craftdig.Menus.Singleplayer;

[Player]
public class PlayerSingleplayerUnloadWorldAction(WorldScope worldScope, WorldDimensionBag dimensionBag, PlayerMetrics metrics)
{
    public void Run()
    {
        metrics.Stop();

        foreach (var dimension in dimensionBag.Ents)
        {
            var dimensionLoaderScope = dimension.DimensionScope.Scope<DimensionLoaderScope>();
            dimensionLoaderScope.Get<DimensionFrontendUnloader>().Run();
            dimensionLoaderScope.Get<DimensionBackendUnloader>().Run();
            dimensionLoaderScope.Get<DimensionUnloader>().Run();
        }

        var worldLoaderScope = worldScope.Scope<WorldLoaderScope>();
        worldLoaderScope.Get<WorldFrontendUnloader>().Run();
        worldLoaderScope.Get<WorldBackendUnloader>().Run();
        worldLoaderScope.Get<WorldUnloader>().Run();
    }
}
