namespace Crafthoe.Menus;

[Module]
public class ModuleMultiPlayerJoinAction(RootState state, ModuleEnts ents, ModuleScope scope)
{
    public void Run(string host, int port, Socket socket)
    {
        var worldScope = scope.Scope<WorldScope>();

        var worldLoaderScope = worldScope.Scope<WorldLoaderScope>();
        worldLoaderScope.Get<WorldLoader>().Run();

        var dimensionScope = worldScope.Scope<DimensionScope>();

        var dimensionEnt = new EntPtr()
            .DimensionScope(dimensionScope);
        worldScope.Get<WorldDimensionBag>().Add(dimensionEnt);

        // For now just find the first dimension
        var dimension = ents.Set.First(x => x.IsDimension());

        dimensionScope.Add(new DimensionAir(dimension.Air()));
        dimensionScope.Get<DimensionDrawDistance>().Far = 24;

        var dimensionLoaderScope = dimensionScope.Scope<DimensionLoaderScope>();
        dimensionLoaderScope.Get<DimensionLoader>().Run();
        dimensionLoaderScope.Get<DimensionFrontendLoader>().Run();

        var players = dimensionScope.Get<DimensionPlayerBag>();
        var player = new EntObj();
        players.Add(player);

        var playerScope = dimensionScope.Scope<PlayerScope>();
        playerScope.Add(new PlayerEnt(player));
        playerScope.Add(new PlayerSocket(socket));
        playerScope.Get<PlayerSocketLoop>().Start();
        playerScope.Get<PlayerMultiPlayerSpawnAction>().Run();

        state.Current = playerScope.New<PlayerMultiPlayerState>();
    }
}
