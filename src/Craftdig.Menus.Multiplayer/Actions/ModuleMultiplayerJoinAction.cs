namespace Craftdig.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerJoinAction(RootState state, ModuleEnts ents, ModuleScope scope)
{
    public void Run(PlayerSocket socket)
    {
        socket.Tag = "sc";

        var worldScope = scope.Scope<WorldScope>();

        var worldLoaderScope = worldScope.Scope<WorldLoaderScope>();
        worldLoaderScope.Get<WorldLoader>().Run();

        var dimensionScope = worldScope.Scope<DimensionScope>();

        var dimensionEnt = worldScope.Get<WorldEntArena>().Alloc().Mutate()
            .DimensionScope(dimensionScope)
            .IsDimensionScope(true)
            .IsLoaded(true);

        // For now just find the first dimension
        var dimension = ents.Set.First(x => x.IsDimension);

        dimensionScope.Add(new DimensionEnt(dimension));
        dimensionScope.Get<DimensionChunkUnloaderHandlers>().Add(dimensionScope.Get<DimensionChunkFrontendUnloader>().Unload);

        var dimensionLoaderScope = dimensionScope.Scope<DimensionLoaderScope>();
        dimensionLoaderScope.Get<DimensionLoader>().Run();
        dimensionLoaderScope.Get<DimensionFrontendLoader>().Run();

        var player = dimensionScope.Get<DimensionEntArena>().Alloc();
        var playerScope = dimensionScope.Scope<PlayerScope>();
        playerScope.Add(new PlayerEnt(player));
        playerScope.Add(socket);
        dimensionScope.Get<DimensionChunkUnloaderHandlers>().Add(playerScope.Get<PlayerChunkClientUnloader>().Unload);
        playerScope.Get<PlayerSocketLoop>().Start();
        playerScope.Get<PlayerMultiplayerSpawnAction>().Run();

        state.Current = playerScope.New<PlayerMultiplayerState>();
    }
}
