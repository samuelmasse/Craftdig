namespace Craftdig;

public class ServerPingTask
{
    public required ServerAddress Address { get; init; }
    public CancellationTokenSource Token { get; } = new();
    public Thread? Thread { get; set; }
    public NetSocket? Socket { get; set; }
    public ServerPingResult? Result { get; set; }
}
