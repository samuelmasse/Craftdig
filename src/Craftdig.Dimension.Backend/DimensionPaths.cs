namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionPaths(WorldPaths paths)
{
    public string Regions { get; } = Path.Join(paths.Root, "Regions");
    public string Ents { get; } = Path.Join(paths.Root, "Ents");
}
