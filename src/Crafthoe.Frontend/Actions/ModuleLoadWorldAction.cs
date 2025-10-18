namespace Crafthoe.Frontend;

[Module]
public class ModuleLoadWorldAction(RootState state, ModuleEnts ents, ModuleScope scope, ModuleReadWorldMetaAction readWorldMetaAction)
{
    public void Run(WorldPaths paths)
    {
        var metadata = readWorldMetaAction.Read(paths);

        var worldScope = scope.Scope<WorldScope>();
        worldScope.Add(paths);
        worldScope.Add(metadata);
        worldScope.Scope<WorldLoaderScope>().Get<WorldLoader>().Run();

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

        var players = dimensionScope.Get<DimensionPlayerBag>();
        var player = new EntObj();
        players.Add((EntMut)player);

        var playerScope = dimensionScope.Scope<PlayerScope>();
        playerScope.Add(new PlayerEnt(player));

        state.Current = playerScope.New<PlayerState>();
    }
}
