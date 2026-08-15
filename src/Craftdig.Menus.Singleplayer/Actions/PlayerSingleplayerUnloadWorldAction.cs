namespace Craftdig;

[Player]
public class PlayerSingleplayerUnloadWorldAction(
    WorldScope worldScope,
    PlayerScope playerScope,
    WorldDimensionBag dimensionBag,
    PlayerMetrics metrics,
    InjectorScopeGraph graph)
{
    public void Run()
    {
        metrics.Stop();
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
