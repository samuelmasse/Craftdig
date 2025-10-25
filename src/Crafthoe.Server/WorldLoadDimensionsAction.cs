namespace Crafthoe.Server;

[World]
public class WorldLoadDimensionsAction(ModuleEnts ents, WorldScope worldScope)
{
    public void Run()
    {
        worldScope.Scope<WorldLoaderScope>().Get<WorldLoader>().Run();

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

        var dimensionLoaderScope = dimensionScope.Scope<DimensionLoaderScope>();
        dimensionLoaderScope.Get<DimensionLoader>().Run();

        var players = dimensionScope.Get<DimensionPlayerBag>();
        var player = new EntObj();
        players.Add((EntMut)player);
    }
}
