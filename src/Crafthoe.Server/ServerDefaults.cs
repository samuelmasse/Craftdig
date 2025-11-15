namespace Crafthoe.Server;

[Server]
public record class ServerDefaults
{
    public required string Name { get; init; }
    public required Ent GameMode { get; init; }
    public required Ent Difficulty { get; init; }
    public int? Seed { get; init; }
    public string[] Allowlist { get; init; } = [];
    public bool NoAuth { get; init; }
    public bool DisableTls { get; init;}
    public bool EnableRawTcp { get; init;}
}
