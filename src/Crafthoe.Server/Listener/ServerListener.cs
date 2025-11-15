namespace Crafthoe.Server;

[Server]
public class ServerListener(ServerDefaults defaults, ServerListenerLoop listenerLoop)
{
    private Thread? thread;
    private Action? stop;

    public void Start()
    {
        if (!defaults.EnableRawTcp)
            return;

        int port = 36677;

        (thread, stop) = listenerLoop.Run(port, (tcp) => new(tcp, tcp.GetStream()));
        thread.Start();

        Console.WriteLine($"Listening on port {port}...");
    }

    public void Stop() => stop?.Invoke();

    public void Join()
    {
        thread?.Join();
        Console.WriteLine("Listener stopped");
    }
}
