namespace Craftdig;

[Server]
public class ServerListenerLoop(Log log, ServerClientLoop clientLoop, ServerClientLimits clientLimits)
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
                TcpClient? tcp = null;

                try
                {
                    clientLimits.Wait();
                    log.Debug("Listener on port {0} accepting new connections", port);
                    tcp = listener.AcceptTcpClient();
                    log.Debug("Listener on port {0} got a new connection {1}", port, tcp.Client.RemoteEndPoint);
                    tcp.NoDelay = true;
                    clientLoop.Start(handler(tcp));
                }
                catch (AuthenticationException e)
                {
                    log.Warn(
                        "Listener on port {0} failed the TLS handshake for socket {1}: {2}",
                        port,
                        tcp?.Client.RemoteEndPoint,
                        e.Message);
                    tcp?.Dispose();
                }
                catch (Exception e)
                {
                    if (stop)
                    {
                        tcp?.Dispose();
                        return;
                    }
                    else
                    {
                        if (tcp == null)
                            log.Error("Listener on port {0} encountered an error", port, e);
                        else log.Error("Listener on port {0} encountered an error with socket {1}",
                            port, tcp.Client.RemoteEndPoint, e);
                    }
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
