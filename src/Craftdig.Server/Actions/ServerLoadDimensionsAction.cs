namespace Craftdig.Server;

[Server]
public class ServerLoadDimensionsAction(ModuleEnts ents, WorldScope worldScope, WorldEntArena arena)
{
    public void Run()
    {
        worldScope.Scope<WorldLoaderScope>()
            .Run(x => x.Get<WorldLoader>().Run())
            .Run(x => x.Get<WorldBackendLoader>().Run())
            .Run(x => x.Get<WorldServerLoader>().Run());

        // For now just find the first dimension
        var dimension = ents.Set.First(x => x.IsDimension);
        worldScope.Scope<DimensionScope>()
            .With(new DimensionEnt(dimension))
            .With(x => new DimensionTerrainGenerator((ITerrainGenerator)x.Get(dimension.TerrainGeneratorType)))
            .With(x => new DimensionBiomeGenerator((IBiomeGenerator)x.Get(dimension.BiomeGeneraetorType)))
            .Run(x => x.Get<DimensionChunkReceiverHandlers>().Add(x.Get<DimensionEntChunkBackendReceiver>().Receive))
            .Run(x => x.Get<DimensionChunkUnloaderHandlers>().Add(x.Get<DimensionChunkBackendUnloader>().Unload))
            .Run(x => x.Get<DimensionChunkUnloaderHandlers>().Add(x.Get<DimensionEntChunkBackendUnloader>().Unload))
            .Run(x => x.Scope<DimensionLoaderScope>()
                .Run(x => x.Get<DimensionLoader>().Run())
                .Run(x => x.Get<DimensionBackendLoader>().Run())
                .Run(x => x.Get<DimensionServerLoader>().Run()))
            .Run(x => arena.Alloc().Mutate()
                .DimensionScope(x)
                .IsDimensionScope(true)
                .IsLoaded(true));
    }
}
