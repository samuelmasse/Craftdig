namespace Craftdig;

public class ServerPingTask(ServerAddress address)
{
    private readonly CancellationTokenSource token = new();
    private readonly TcpClient tcp = new() { NoDelay = true };
    private Thread? thread;
    private NetSocket? socket;
    private ServerPingResult? result;

    public ServerAddress Address => address;
    public CancellationTokenSource Token => token;
    public TcpClient Tcp => tcp;
    public ref Thread? Thread => ref thread;
    public ref NetSocket? Socket => ref socket;
    public ref ServerPingResult? Result => ref result;
}
