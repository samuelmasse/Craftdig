namespace Crafthoe.Server;

[Server]
public class ServerLoadDimensionsAction(ModuleEnts ents, WorldScope worldScope)
{
    public void Run()
    {
        var worldLoaderScope = worldScope.Scope<WorldLoaderScope>();
        worldLoaderScope.Get<WorldLoader>().Run();
        worldLoaderScope.Get<WorldBackendLoader>().Run();

        var dimensionScope = worldScope.Scope<DimensionScope>();
        var dimensionEnt = new EntPtr().DimensionScope(dimensionScope);
        worldScope.Get<WorldDimensionBag>().Add(dimensionEnt);

        // For now just find the first dimension
        var dimension = ents.Set.First(x => x.IsDimension());

        dimensionScope.Add(new DimensionAir(dimension.Air()));
        dimensionScope.Add(new DimensionTerrainGenerator(
            (ITerrainGenerator)dimensionScope.Get(dimension.TerrainGeneratorType())));
        dimensionScope.Add(new DimensionBiomeGenerator(
            (IBiomeGenerator)dimensionScope.Get(dimension.BiomeGeneraetorType())));

        dimensionScope.Get<DimensionChunkUnloaderHandlers>().Add(dimensionScope.Get<DimensionChunkBackendUnloader>().Unload);

        var dimensionLoaderScope = dimensionScope.Scope<DimensionLoaderScope>();
        dimensionLoaderScope.Get<DimensionLoader>().Run();
        dimensionLoaderScope.Get<DimensionBackendLoader>().Run();
    }
}
