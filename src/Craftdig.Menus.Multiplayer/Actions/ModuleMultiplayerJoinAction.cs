namespace Craftdig.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerJoinAction(RootState state, ModuleEnts ents, ModuleScope scope)
{
    public void Run(PlayerSocket socket)
    {
        socket.Ent.Tag() = "sc";

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
        dimensionScope.Get<DimensionChunkUnloaderHandlers>().Add(dimensionScope.Get<DimensionChunkFrontendUnloader>().Unload);

        var dimensionLoaderScope = dimensionScope.Scope<DimensionLoaderScope>();
        dimensionLoaderScope.Get<DimensionLoader>().Run();
        dimensionLoaderScope.Get<DimensionFrontendLoader>().Run();

        var players = dimensionScope.Get<DimensionPlayerBag>();
        var player = new EntObj();
        players.Add(player);

        var playerScope = dimensionScope.Scope<PlayerScope>();
        playerScope.Add(new PlayerEnt(player));
        playerScope.Add(socket);
        dimensionScope.Get<DimensionChunkUnloaderHandlers>().Add(playerScope.Get<PlayerChunkClientUnloader>().Unload);
        playerScope.Get<PlayerSocketLoop>().Start();
        playerScope.Get<PlayerMultiplayerSpawnAction>().Run();

        state.Current = playerScope.New<PlayerMultiplayerState>();
    }
}
