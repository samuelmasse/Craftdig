namespace Craftdig.Menus.Multiplayer;

[Player]
public class PlayerMultiplayerDisconnectAction(
    WorldScope worldScope,
    WorldDimensionBag dimensionBag,
    PlayerPresenceClient presenceClient,
    PlayerIdentityRefresh identityRefresh,
    PlayerIdentitySession identitySession,
    PlayerSocketLoop socketLoop,
    PlayerEntUpdateQueue entUpdates)
{
    public void Run()
    {
        presenceClient.Stop();
        identityRefresh.Stop();
        socketLoop.Stop();
        identitySession.Dispose();
        entUpdates.Clear();

        foreach (var dimension in dimensionBag.Ents)
        {
            var dimensionLoaderScope = dimension.DimensionScope.Scope<DimensionLoaderScope>();
            dimensionLoaderScope.Get<DimensionFrontendUnloader>().Run();
            dimensionLoaderScope.Get<DimensionUnloader>().Run();
        }

        worldScope.Scope<WorldLoaderScope>().Get<WorldFrontendUnloader>().Run();
        worldScope.Scope<WorldLoaderScope>().Get<WorldUnloader>().Run();
    }
}
