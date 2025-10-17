namespace Crafthoe.Dimension;

[Dimension]
public class DimensionPaths(WorldPaths paths)
{
    public string Regions { get; } = Path.Join(paths.Root, "Regions");
}
