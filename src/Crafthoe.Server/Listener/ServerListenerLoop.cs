namespace Crafthoe.Server;

[Server]
public class ServerListenerLoop(ServerClientLoop clientLoop)
{
    public (Thread, Action) Run(int port, Func<TcpClient, NetSocket> handler)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start(10);
        var stop = false;

        return (new Thread(() =>
        {
            while (true)
            {
                try
                {
                    var tcp = listener.AcceptTcpClient();
                    tcp.NoDelay = true;
                    clientLoop.Start(handler(tcp));
                }
                catch
                {
                    if (stop)
                        return;
                    else throw;
                }
            }
        }), () =>
        {
            stop = true;
            listener.Dispose();
        }
        );
    }
}
