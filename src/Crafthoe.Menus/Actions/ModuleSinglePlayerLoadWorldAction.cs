namespace Crafthoe.Menus;

[Module]
public class ModuleSinglePlayerLoadWorldAction(RootState state, ModuleEnts ents, ModuleScope scope, ModuleReadWorldMetaAction readWorldMetaAction)
{
    public void Run(WorldPaths paths)
    {
        var metadata = readWorldMetaAction.Read(paths);

        var worldScope = scope.Scope<WorldScope>();
        worldScope.Add(paths);
        worldScope.Add(metadata);

        var worldLoaderScope = worldScope.Scope<WorldLoaderScope>();
        worldLoaderScope.Get<WorldLoader>().Run();
        worldLoaderScope.Get<WorldServerLoader>().Run();

        var dimensionScope = worldScope.Scope<DimensionScope>();

        var dimensionEnt = new EntPtr()
            .DimensionScope(dimensionScope);
        worldScope.Get<WorldDimensionBag>().Add(dimensionEnt);

        // For now just find the first dimension
        var dimension = ents.Set.First(x => x.IsDimension());

        dimensionScope.Add(new DimensionAir(dimension.Air()));
        dimensionScope.Add(new DimensionTerrainGenerator(
            (ITerrainGenerator)dimensionScope.Get(dimension.TerrainGeneratorType())));
        dimensionScope.Add(new DimensionBiomeGenerator(
            (IBiomeGenerator)dimensionScope.Get(dimension.BiomeGeneraetorType())));

        var dimensionLoaderScope = dimensionScope.Scope<DimensionLoaderScope>();
        dimensionLoaderScope.Get<DimensionLoader>().Run();
        dimensionLoaderScope.Get<DimensionServerLoader>().Run();
        dimensionLoaderScope.Get<DimensionClientLoader>().Run();

        dimensionScope.Get<DimensionDrawDistance>().Far = dimensionScope.Get<DimensionChunkRequester>().Far;
        dimensionScope.Get<DimensionChunkReceiverHandlers>().Add(dimensionScope.Get<DimensionClientChunkReceiverHandler>().Handle);
        dimensionScope.Get<DimensionChunkUnloaderHandlers>().Add(dimensionScope.Get<DimensionClientChunkUnloaderHandler>().Handle);

        var players = dimensionScope.Get<DimensionPlayerBag>();
        var player = new EntObj();
        players.Add(player);

        var playerScope = dimensionScope.Scope<PlayerScope>();
        playerScope.Add(new PlayerEnt(player));

        state.Current = playerScope.New<PlayerSinglePlayerState>();
    }
}
