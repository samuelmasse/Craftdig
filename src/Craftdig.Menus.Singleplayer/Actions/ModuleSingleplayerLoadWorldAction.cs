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

        dimensionScope.Add(new DimensionEnt(dimension));
        dimensionScope.Add(new DimensionTerrainGenerator((ITerrainGenerator)dimensionScope.Get(dimension.TerrainGeneratorType)));
        dimensionScope.Add(new DimensionBiomeGenerator((IBiomeGenerator)dimensionScope.Get(dimension.BiomeGeneraetorType)));
        dimensionScope.Get<DimensionChunkReceiverHandlers>().Add(dimensionScope.Get<DimensionChunkFrontendReceiver>().Receive);
        dimensionScope.Get<DimensionChunkReceiverHandlers>().Add(dimensionScope.Get<DimensionEntChunkBackendReceiver>().Receive);
        dimensionScope.Get<DimensionChunkUnloaderHandlers>().Add(dimensionScope.Get<DimensionChunkBackendUnloader>().Unload);
        dimensionScope.Get<DimensionChunkUnloaderHandlers>().Add(dimensionScope.Get<DimensionChunkFrontendUnloader>().Unload);
        dimensionScope.Get<DimensionChunkUnloaderHandlers>().Add(dimensionScope.Get<DimensionEntChunkBackendUnloader>().Unload);

        var dimensionLoaderScope = dimensionScope.Scope<DimensionLoaderScope>();
        dimensionLoaderScope.Get<DimensionLoader>().Run();
        dimensionLoaderScope.Get<DimensionBackendLoader>().Run();
        dimensionLoaderScope.Get<DimensionFrontendLoader>().Run();

        var worldEntRegion = worldScope.Get<WorldEntRegionStates>()[default];
        var existingPlayer = worldEntRegion.Ents.FirstOrDefault(x => x.IsWorldPlayer);

        EntMutIdx player = default;

        if (existingPlayer != default)
        {
            var rloc = existingPlayer.WorldPosition.ToLoc().Xy.ToCloc().ToRloc();
            var dimensionEntRegion = dimensionScope.Get<DimensionEntRegionStates>()[rloc];
            player = dimensionEntRegion.Ents.First(x => x.WorldPlayer == existingPlayer);
        }
        else player = dimensionScope.Get<DimensionEntArena>().Alloc();

        var playerScope = dimensionScope.Scope<PlayerScope>();
        playerScope.Get<PlayerMetrics>().Start();
        playerScope.Add(new PlayerEnt(player));

        state.Current = playerScope.New<PlayerSingleplayerState>();
    }
}
