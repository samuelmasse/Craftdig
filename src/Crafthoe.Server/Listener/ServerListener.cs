namespace Crafthoe.Server;

[Server]
public class ServerListener(ServerListenerLoop listenerLoop)
{
    private Thread? thread;
    private Action? stop;

    public void Start()
    {
        int port = 36677;

        (thread, stop) = listenerLoop.Run(port, (tcp) => new(tcp, tcp.GetStream()));
        thread.Start();

        Console.WriteLine($"Listening on port {port}...");
    }

    public void Stop() => stop?.Invoke();

    public void Join()
    {
        Console.WriteLine("Listener stopped");
        thread?.Join();
    }
}
