namespace Crafthoe.Server;

[Server]
public class ServerConfig
{
    public bool? DisableTls { get; init; }
    public bool? EnableRawTcp { get; init; }
    public string? CertPath { get; init; }
    public string? KeyPath { get; init; }
}
