namespace Crafthoe.Frontend;

[Player]
public class PlayerUnloadWorldAction(WorldScope worldScope, WorldDimensionBag dimensionBag)
{
    public void Run()
    {
        foreach (var dimension in dimensionBag.Ents)
        {
            dimension.DimensionScope().Scope<DimensionLoaderScope>().Get<DimensionUnloader>().Run();
            dimension.Dispose();
        }

        worldScope.Scope<WorldLoaderScope>().Get<WorldUnloader>().Run();
    }
}
