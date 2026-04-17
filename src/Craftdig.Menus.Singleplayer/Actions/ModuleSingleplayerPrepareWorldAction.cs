namespace Craftdig.Menus.Singleplayer;

[Module]
public class ModuleSingleplayerPrepareWorldAction(
    ModuleEnts ents,
    ModuleScope scope,
    ModuleReadWorldMetaAction readWorldMetaAction)
{
    public DimensionScope Run(WorldPaths paths)
    {
        var worldScope = scope.Scope<WorldScope>()
            .With(paths)
            .With(readWorldMetaAction.Read(paths))
            .Run(x => x.Scope<WorldLoaderScope>()
                .Run(x => x.Get<WorldLoader>().Run())
                .Run(x => x.Get<WorldBackendLoader>().Run()));

        // For now just find the first dimension
        var dimension = ents.Set.First(x => x.IsDimension);
        return worldScope.Scope<DimensionScope>()
            .With(new DimensionEnt(dimension))
            .With(x => new DimensionTerrainGenerator((ITerrainGenerator)x.Get(dimension.TerrainGeneratorType)))
            .With(x => new DimensionBiomeGenerator((IBiomeGenerator)x.Get(dimension.BiomeGeneraetorType)))
            .Run(x => x.Get<DimensionChunkReceiverHandlers>().Add(x.Get<DimensionChunkFrontendReceiver>().Receive))
            .Run(x => x.Get<DimensionChunkReceiverHandlers>().Add(x.Get<DimensionEntChunkBackendReceiver>().Receive))
            .Run(x => x.Get<DimensionChunkUnloaderHandlers>().Add(x.Get<DimensionChunkBackendUnloader>().Unload))
            .Run(x => x.Get<DimensionChunkUnloaderHandlers>().Add(x.Get<DimensionChunkFrontendUnloader>().Unload))
            .Run(x => x.Get<DimensionChunkUnloaderHandlers>().Add(x.Get<DimensionEntChunkBackendUnloader>().Unload))
            .Run(x => x.Scope<DimensionLoaderScope>()
                .Run(x => x.Get<DimensionLoader>().Run())
                .Run(x => x.Get<DimensionBackendLoader>().Run())
                .Run(x => x.Get<DimensionFrontendLoader>().Run()))
            .Run(x => worldScope.Get<WorldEntArena>().Alloc().Mutate()
                .DimensionScope(x)
                .IsDimensionScope(true)
                .IsLoaded(true));
    }
}
