namespace Craftdig;

[Server]
public class ServerConfig
{
    public string? RootPath { get; init; }
    public bool PublicServer { get; init; }
    public bool? DisableTls { get; init; }
    public bool? EnableRawTcp { get; init; }
    public int MaxPlayers { get; init; } = 20;
    public string? Description { get; init; }
    public string? CertPath { get; init; }
    public string? KeyPath { get; init; }
    public string[] PublicServerContexts { get; init; } = [];
    public string IdentityJwksUrl { get; init; } = "https://craftdig.io/.well-known/jwks.json";
    public int PresenceEgressBytesPerSecond { get; init; } = 20_000_000;
    public LogLevel LogLevel { get; init; } = LogLevel.Debug;
}
