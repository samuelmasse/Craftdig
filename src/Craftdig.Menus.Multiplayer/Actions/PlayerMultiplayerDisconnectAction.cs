namespace Craftdig;

[Player]
public class PlayerMultiplayerDisconnectAction(
    WorldScope worldScope,
    PlayerScope playerScope,
    WorldDimensionBag dimensionBag,
    PlayerPresenceClient presenceClient,
    PlayerIdentityRefresh identityRefresh,
    PlayerIdentitySession identitySession,
    PlayerSocketLoop socketLoop,
    PlayerEntUpdateQueue entUpdates,
    InjectorScopeGraph graph)
{
    public void Run()
    {
        presenceClient.Stop();
        identityRefresh.Stop();
        socketLoop.Stop();
        identitySession.Dispose();
        entUpdates.Clear();
        graph.End(playerScope);

        foreach (var dimension in dimensionBag.Ents)
        {
            graph.End(
                dimension.DimensionScope,
                ending =>
                {
                    var loader =
                        ending.Scope<DimensionLoaderScope>();
                    loader.Get<DimensionFrontendUnloader>().Run();
                    loader.Get<DimensionUnloader>().Run();
                });
        }

        graph.End(
            worldScope,
            ending =>
            {
                var loader =
                    ending.Scope<WorldLoaderScope>();
                loader.Get<WorldFrontendUnloader>().Run();
                loader.Get<WorldUnloader>().Run();
            });
    }
}
