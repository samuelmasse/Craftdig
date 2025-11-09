namespace Crafthoe.Server;

[Server]
public record class ServerDefaults
{
    public required string Name { get; init; }
    public required Ent GameMode { get; init; }
    public required Ent Difficulty { get; init; }
    public int? Seed { get; init; }
}
