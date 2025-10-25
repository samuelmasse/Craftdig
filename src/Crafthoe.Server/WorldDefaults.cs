namespace Crafthoe.Server;

[World]
public record class WorldDefaults
{
    public required string Name { get; init; }
    public required Ent GameMode { get; init; }
    public required Ent Difficulty { get; init; }
    public int? Seed { get; init; }
}
