namespace Crafthoe.Server;

[Server]
public class ServerUnloadDimensionsAction(
    WorldScope worldScope,
    WorldDimensionBag dimensionBag)
{
    public void Run()
    {
        foreach (var dimension in dimensionBag.Ents)
        {
            var dimensionLoaderScope = dimension.DimensionScope().Scope<DimensionLoaderScope>();
            dimensionLoaderScope.Get<DimensionBackendUnloader>().Run();
            dimensionLoaderScope.Get<DimensionUnloader>().Run();
            dimension.Dispose();
        }

        worldScope.Scope<WorldLoaderScope>().Get<WorldUnloader>().Run();

        Console.WriteLine("Dimensions unloaded");
    }
}
