namespace Craftdig;

[Dimension]
public class DimensionPaths(WorldPaths paths, DimensionEnt dimension)
{
    public string Regions { get; } = Path.Join(paths.Regions, dimension.Name);
    public string Ents { get; } = Path.Join(paths.Ents, dimension.Name);
}
