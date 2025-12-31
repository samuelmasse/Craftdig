namespace Craftdig.Server;

[Server]
public record class ServerDefaults
{
    public string[] Allowlist { get; init; } = [];
    public bool NoAuth { get; init; }
    public bool DisableTls { get; init; }
    public bool EnableRawTcp { get; init; }
}
