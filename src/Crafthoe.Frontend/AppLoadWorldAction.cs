namespace Crafthoe.Frontend;

[App]
public class AppLoadWorldAction(RootState state, AppScope scope)
{
    public void Run()
    {
        var moduleScope = scope.Scope<ModuleScope>();
        var worldScope = moduleScope.Scope<WorldScope>();
        worldScope.Scope<WorldLoaderScope>().Get<WorldLoader>().Run();

        var dimensionScope = worldScope.Scope<DimensionScope>();

        dimensionScope.Add(new DimensionAir(moduleScope.Get<ModuleBlocks>().Air));
        dimensionScope.Add(new DimensionTerrainGenerator(
            dimensionScope.Get<DimensionOverworldTerrainGenerator>()));
        dimensionScope.Add(new DimensionBiomeGenerator(
            dimensionScope.Get<DimensionOverworldBiomeGenerator>()));

        var players = dimensionScope.Get<DimensionPlayerBag>();
        var player = new EntObj();
        players.Add((EntMut)player);

        var playerScope = dimensionScope.Scope<PlayerScope>();
        playerScope.Add(new PlayerEnt(player));

        state.Current = playerScope.New<PlayerState>();
    }
}
