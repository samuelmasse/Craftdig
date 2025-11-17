namespace Crafthoe.Server;

[Server]
public class ServerConfig
{
    public string? RootPath { get; init; }
    public bool PublicServer { get; init; }
    public bool? DisableTls { get; init; }
    public bool? EnableRawTcp { get; init; }
    public string? CertPath { get; init; }
    public string? KeyPath { get; init; }
}
