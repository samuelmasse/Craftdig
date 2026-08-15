namespace Craftdig;

[Server]
public class ServerUnloadDimensionsAction(
    Log log,
    WorldScope worldScope,
    WorldDimensionBag dimensionBag)
{
    public void Run()
    {
        foreach (var dimension in dimensionBag.Ents)
        {
            var dimensionLoaderScope = dimension.DimensionScope.Scope<DimensionLoaderScope>();
            dimensionLoaderScope.Get<DimensionBackendUnloader>().Run();
            dimensionLoaderScope.Get<DimensionUnloader>().Run();
        }

        var worldLoaderScope = worldScope.Scope<WorldLoaderScope>();
        worldLoaderScope.Get<WorldBackendUnloader>().Run();
        worldLoaderScope.Get<WorldUnloader>().Run();

        log.Info("Dimensions unloaded");
    }
}
