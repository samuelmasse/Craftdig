namespace Crafthoe.Server;

[Server]
public class ServerListener(ServerListenerLoop listenerLoop)
{
    private Thread? thread;

    public void Start()
    {
        int port = 36677;

        thread = listenerLoop.Run(port, (tcp) => new(tcp, tcp.GetStream()));
        thread.Start();

        Console.WriteLine($"Listening on port {port}...");
    }


    public void Join() => thread?.Join();
}
