namespace Craftdig;

[World]
public record WorldPaths(string Root)
{
    public string Regions { get; } = Path.Join(Root, "Regions");
    public string Ents { get; } = Path.Join(Root, "Ents");
}
