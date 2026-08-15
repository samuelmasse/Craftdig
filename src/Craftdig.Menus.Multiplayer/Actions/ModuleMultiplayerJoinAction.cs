namespace Craftdig;

[Module]
public class ModuleMultiplayerJoinAction(
    RootState state,
    ModuleEnts ents,
    ModuleScope scope,
    InjectorScopeGraph graph)
{
    public void Run(PlayerSocket socket, PlayerIdentitySession identitySession)
    {
        socket.Tag = "sc";

        var worldScope = graph.Scope<WorldScope>(
            scope,
            "Remote world");
        graph.Run<WorldLoaderScope>(
            worldScope,
            loader => loader.Get<WorldLoader>().Run(),
            "World load");

        // For now just find the first dimension
        var dimension = ents.Set.First(x => x.IsDimension);
        var dimensionScope = graph.Scope<DimensionScope>(
                worldScope,
                dimension.Name)
            .With(new DimensionEnt(dimension))
            .Run(x => x.Get<DimensionChunkUnloaderHandlers>().Add(x.Get<DimensionChunkFrontendUnloader>().Unload));
        graph.Run<DimensionLoaderScope>(
            dimensionScope,
            loader =>
            {
                loader.Get<DimensionLoader>().Run();
                loader.Get<DimensionFrontendLoader>().Run();
            },
            "Dimension load");
        worldScope.Get<WorldEntArena>().Alloc().Mutate()
            .DimensionScope(dimensionScope)
            .IsDimensionScope(true)
            .IsLoaded(true);

        graph.Scope<PlayerScope>(
                dimensionScope,
                "Multiplayer player")
            .With(new PlayerEnt(dimensionScope.Get<DimensionEntArena>().Alloc()))
            .With(socket)
            .With(identitySession)
            .Run(x => dimensionScope.Get<DimensionChunkUnloaderHandlers>().Add(x.Get<PlayerChunkClientUnloader>().Unload))
            .Run(x => x.Get<PlayerInventoryActions>().Enable())
            .Run(x => x.Get<PlayerIdentityRefresh>().Start())
            .Run(x => x.Get<PlayerPresenceClient>().Start())
            .Run(x => x.Get<PlayerSocketLoop>().Start())
            .Run(x => x.Get<PlayerMultiplayerSpawnAction>().Run())
            .Run(x => state.Current = x.New<PlayerMultiplayerLoadingState>());
    }
}
