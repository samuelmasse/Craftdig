namespace Craftdig.Menus.Singleplayer;

[Module]
public class ModuleSingleplayerLoadWorldAction(
    RootState state,
    ModuleEnts ents,
    ModuleScope scope,
    ModuleReadWorldMetaAction readWorldMetaAction)
{
    public void Run(WorldPaths paths)
    {
        var metadata = readWorldMetaAction.Read(paths);

        var worldScope = scope.Scope<WorldScope>();
        worldScope.Add(paths);
        worldScope.Add(metadata);

        var worldLoaderScope = worldScope.Scope<WorldLoaderScope>();
        worldLoaderScope.Get<WorldLoader>().Run();
        worldLoaderScope.Get<WorldBackendLoader>().Run();

        var dimensionScope = worldScope.Scope<DimensionScope>();

        var dimensionEnt = worldScope.Get<WorldEntArena>().Alloc().Mutate()
            .DimensionScope(dimensionScope)
            .IsDimensionScope(true)
            .IsLoaded(true);

        // For now just find the first dimension
        var dimension = ents.Set.First(x => x.IsDimension);

        dimensionScope.Add(new DimensionAir(dimension.Air));
        dimensionScope.Add(new DimensionTerrainGenerator(
            (ITerrainGenerator)dimensionScope.Get(dimension.TerrainGeneratorType)));
        dimensionScope.Add(new DimensionBiomeGenerator(
            (IBiomeGenerator)dimensionScope.Get(dimension.BiomeGeneraetorType)));

        dimensionScope.Get<DimensionChunkReceiverHandlers>().Add(dimensionScope.Get<DimensionChunkFrontendReceiver>().Receive);
        dimensionScope.Get<DimensionChunkUnloaderHandlers>().Add(dimensionScope.Get<DimensionChunkBackendUnloader>().Unload);
        dimensionScope.Get<DimensionChunkUnloaderHandlers>().Add(dimensionScope.Get<DimensionChunkFrontendUnloader>().Unload);

        var dimensionLoaderScope = dimensionScope.Scope<DimensionLoaderScope>();
        dimensionLoaderScope.Get<DimensionLoader>().Run();
        dimensionLoaderScope.Get<DimensionBackendLoader>().Run();
        dimensionLoaderScope.Get<DimensionFrontendLoader>().Run();

        var player = dimensionScope.Get<DimensionEntArena>().Alloc().Mutate()
            .IsPlayer(true)
            .IsLoaded(true);

        var playerScope = dimensionScope.Scope<PlayerScope>();
        playerScope.Get<PlayerMetrics>().Start();
        playerScope.Add(new PlayerEnt(player.Ent));

        state.Current = playerScope.New<PlayerSingleplayerState>();
    }
}
