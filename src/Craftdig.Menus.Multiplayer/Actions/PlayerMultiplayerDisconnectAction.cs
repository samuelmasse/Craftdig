namespace Craftdig.Menus.Multiplayer;

[Player]
public class PlayerMultiplayerDisconnectAction(
    WorldScope worldScope,
    WorldDimensionBag dimensionBag,
    PlayerSocketLoop socketLoop)
{
    public void Run()
    {
        socketLoop.Stop();

        foreach (var dimension in dimensionBag.Ents)
        {
            var dimensionLoaderScope = dimension.DimensionScope().Scope<DimensionLoaderScope>();
            dimensionLoaderScope.Get<DimensionFrontendUnloader>().Run();
            dimensionLoaderScope.Get<DimensionUnloader>().Run();
            dimension.Dispose();
        }

        worldScope.Scope<WorldLoaderScope>().Get<WorldFrontendUnloader>().Run();
        worldScope.Scope<WorldLoaderScope>().Get<WorldUnloader>().Run();
    }
}
