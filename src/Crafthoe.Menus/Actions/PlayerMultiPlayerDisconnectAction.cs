namespace Crafthoe.Menus;

[Player]
public class PlayerMultiPlayerDisconnectAction(
    WorldScope worldScope,
    WorldDimensionBag dimensionBag,
    PlayerSocket socket,
    PlayerSocketLoop socketLoop)
{
    public void Run()
    {
        socketLoop.Stop();
        socket.Raw.Disconnect(false);
        socket.Raw.Dispose();

        foreach (var dimension in dimensionBag.Ents)
        {
            var dimensionLoaderScope = dimension.DimensionScope().Scope<DimensionLoaderScope>();
            dimensionLoaderScope.Get<DimensionClientUnloader>().Run();
            dimensionLoaderScope.Get<DimensionUnloader>().Run();
            dimension.Dispose();
        }

        worldScope.Scope<WorldLoaderScope>().Get<WorldClientUnloader>().Run();
        worldScope.Scope<WorldLoaderScope>().Get<WorldUnloader>().Run();
    }
}
