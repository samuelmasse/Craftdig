namespace Crafthoe.World;

[World]
public record WorldPaths(string Root)
{
    public readonly string Regions = Path.Join(Root, "Regions");
}
