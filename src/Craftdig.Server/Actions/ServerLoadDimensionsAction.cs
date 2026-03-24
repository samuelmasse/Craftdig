namespace Craftdig.Server;

[Server]
public class ServerLoadDimensionsAction(ModuleEnts ents, WorldScope worldScope, WorldEntArena arena)
{
    public void Run()
    {
        var worldLoaderScope = worldScope.Scope<WorldLoaderScope>();
        worldLoaderScope.Get<WorldLoader>().Run();
        worldLoaderScope.Get<WorldBackendLoader>().Run();
        worldLoaderScope.Get<WorldServerLoader>().Run();

        var dimensionScope = worldScope.Scope<DimensionScope>();
        var dimensionEnt = arena.Alloc().Mutate()
            .DimensionScope(dimensionScope)
            .IsDimensionScope(true)
            .IsLoaded(true);

        // For now just find the first dimension
        var dimension = ents.Set.First(x => x.IsDimension);

        dimensionScope.Add(new DimensionEnt(dimension));
        dimensionScope.Add(new DimensionTerrainGenerator((ITerrainGenerator)dimensionScope.Get(dimension.TerrainGeneratorType)));
        dimensionScope.Add(new DimensionBiomeGenerator((IBiomeGenerator)dimensionScope.Get(dimension.BiomeGeneraetorType)));
        dimensionScope.Get<DimensionChunkReceiverHandlers>().Add(dimensionScope.Get<DimensionEntChunkBackendReceiver>().Receive);
        dimensionScope.Get<DimensionChunkUnloaderHandlers>().Add(dimensionScope.Get<DimensionChunkBackendUnloader>().Unload);
        dimensionScope.Get<DimensionChunkUnloaderHandlers>().Add(dimensionScope.Get<DimensionEntChunkBackendUnloader>().Unload);

        var dimensionLoaderScope = dimensionScope.Scope<DimensionLoaderScope>();
        dimensionLoaderScope.Get<DimensionLoader>().Run();
        dimensionLoaderScope.Get<DimensionBackendLoader>().Run();
        dimensionLoaderScope.Get<DimensionServerLoader>().Run();
    }
}
