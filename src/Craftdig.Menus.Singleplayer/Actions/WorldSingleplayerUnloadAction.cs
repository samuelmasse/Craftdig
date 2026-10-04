namespace Craftdig;

[World]
public class WorldSingleplayerUnloadAction(
    WorldScope worldScope,
    WorldDimensionBag dimensionBag,
    InjectorScopeGraph graph)
{
    private PlayerScope? playerScope;

    public void Attach(PlayerScope player) => playerScope = player;

    public void Run()
    {
        if (playerScope != null)
        {
            playerScope.Get<PlayerMetrics>().Stop();
            graph.End(playerScope);
            playerScope = null;
        }

        foreach (var dimension in dimensionBag.Ents)
        {
            graph.End(
                dimension.DimensionScope,
                ending =>
                {
                    var loader =
                        ending.Scope<DimensionLoaderScope>();
                    loader.Get<DimensionFrontendUnloader>().Run();
                    loader.Get<DimensionBackendUnloader>().Run();
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
                loader.Get<WorldBackendUnloader>().Run();
                loader.Get<WorldUnloader>().Run();
            });
    }
}
