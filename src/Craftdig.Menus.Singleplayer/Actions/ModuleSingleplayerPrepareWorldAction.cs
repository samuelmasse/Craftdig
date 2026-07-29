namespace Craftdig.Menus.Singleplayer;

[Module]
public class ModuleSingleplayerPrepareWorldAction(
    ModuleEnts ents,
    ModuleScope scope,
    InjectorScopeGraph graph,
    ModuleReadWorldMetaAction readWorldMetaAction)
{
    public DimensionScope Run(WorldPaths paths)
    {
        var worldScope = graph.Scope<WorldScope>(
                scope,
                Path.GetFileName(paths.Root))
            .With(paths)
            .With(readWorldMetaAction.Read(paths));
        graph.Run<WorldLoaderScope>(
            worldScope,
            loader =>
            {
                loader.Get<WorldLoader>().Run();
                loader.Get<WorldBackendLoader>().Run();
            },
            "World load");

        // For now just find the first dimension
        var dimension = ents.Set.First(x => x.IsDimension);
        var dimensionScope = graph.Scope<DimensionScope>(
                worldScope,
                dimension.Name)
            .With(new DimensionEnt(dimension))
            .With(x => new DimensionTerrainGenerator((ITerrainGenerator)x.Get(dimension.TerrainGeneratorType)))
            .With(x => new DimensionBiomeGenerator((IBiomeGenerator)x.Get(dimension.BiomeGeneraetorType)))
            .Run(x => x.Get<DimensionChunkReceiverHandlers>().Add(x.Get<DimensionChunkFrontendReceiver>().Receive))
            .Run(x => x.Get<DimensionChunkReceiverHandlers>().Add(x.Get<DimensionEntChunkBackendReceiver>().Receive))
            .Run(x => x.Get<DimensionChunkUnloaderHandlers>().Add(x.Get<DimensionChunkBackendUnloader>().Unload))
            .Run(x => x.Get<DimensionChunkUnloaderHandlers>().Add(x.Get<DimensionChunkFrontendUnloader>().Unload))
            .Run(x => x.Get<DimensionChunkUnloaderHandlers>().Add(x.Get<DimensionEntChunkBackendUnloader>().Unload));
        graph.Run<DimensionLoaderScope>(
            dimensionScope,
            loader =>
            {
                loader.Get<DimensionLoader>().Run();
                loader.Get<DimensionBackendLoader>().Run();
                loader.Get<DimensionFrontendLoader>().Run();
            },
            "Dimension load");
        worldScope.Get<WorldEntArena>().Alloc().Mutate()
            .DimensionScope(dimensionScope)
            .IsDimensionScope(true)
            .IsLoaded(true);
        return dimensionScope;
    }
}
