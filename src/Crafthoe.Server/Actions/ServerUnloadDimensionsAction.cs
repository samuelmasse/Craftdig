namespace Crafthoe.Server;

[Server]
public class ServerUnloadDimensionsAction(
    AppLog log,
    WorldScope worldScope,
    WorldDimensionBag dimensionBag)
{
    public void Run()
    {
        foreach (var dimension in dimensionBag.Ents)
        {
            var dimensionLoaderScope = dimension.DimensionScope().Scope<DimensionLoaderScope>();
            dimensionLoaderScope.Get<DimensionBackendUnloader>().Run();
            dimensionLoaderScope.Get<DimensionUnloader>().Run();
            dimension.Dispose();
        }

        worldScope.Scope<WorldLoaderScope>().Get<WorldUnloader>().Run();

        log.Info("Dimensions unloaded");
    }
}
