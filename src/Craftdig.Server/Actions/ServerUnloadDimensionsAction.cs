namespace Craftdig;

[Server]
public class ServerUnloadDimensionsAction(
    Log log,
    ModuleEntsMut moduleEnts,
    WorldScope worldScope,
    WorldDimensionBag dimensionBag,
    ServerSockets sockets)
{
    public void Run()
    {
        sockets.CollectCompleted();

        foreach (var dimension in dimensionBag.Ents)
        {
            var dimensionLoaderScope = dimension.DimensionScope.Scope<DimensionLoaderScope>();
            dimension.DimensionScope.Get<DimensionServer>().DrainSockets();
            dimensionLoaderScope.Get<DimensionBackendUnloader>().Run();
            dimensionLoaderScope.Get<DimensionUnloader>().Run();
        }

        var worldLoaderScope = worldScope.Scope<WorldLoaderScope>();
        worldLoaderScope.Get<WorldBackendUnloader>().Run();
        worldLoaderScope.Get<WorldUnloader>().Run();
        sockets.ReleaseCompleted();
        moduleEnts.Unload();

        log.Info("Dimensions unloaded");
    }
}
