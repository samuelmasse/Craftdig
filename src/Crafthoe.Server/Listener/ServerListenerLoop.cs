namespace Crafthoe.Server;

[Server]
public class ServerListenerLoop(ServerClientLoop clientLoop)
{
    public Thread Run(int port, Func<TcpClient, NetSocket> handler)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start(10);

        return new Thread(() =>
        {
            while (true)
            {
                var tcp = listener.AcceptTcpClient();
                tcp.NoDelay = true;
                clientLoop.Start(handler(tcp));
            }
        });
    }
}
