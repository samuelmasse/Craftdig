namespace Craftdig.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerJoinAction(RootState state, ModuleEnts ents, ModuleScope scope)
{
    public void Run(PlayerSocket socket, PlayerIdentitySession identitySession)
    {
        socket.Tag = "sc";

        var worldScope = scope.Scope<WorldScope>()
            .Run(x => x.Scope<WorldLoaderScope>()
                .Run(x => x.Get<WorldLoader>().Run()));

        // For now just find the first dimension
        var dimension = ents.Set.First(x => x.IsDimension);
        var dimensionScope = worldScope.Scope<DimensionScope>()
            .With(new DimensionEnt(dimension))
            .Run(x => x.Get<DimensionChunkUnloaderHandlers>().Add(x.Get<DimensionChunkFrontendUnloader>().Unload))
            .Run(x => x.Scope<DimensionLoaderScope>()
                .Run(x => x.Get<DimensionLoader>().Run())
                .Run(x => x.Get<DimensionFrontendLoader>().Run()))
            .Run(x => worldScope.Get<WorldEntArena>().Alloc().Mutate()
                .DimensionScope(x)
                .IsDimensionScope(true)
                .IsLoaded(true));

        dimensionScope.Scope<PlayerScope>()
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
