namespace Craftdig.Server;

[Server]
public class ServerListener(AppLog log, ServerDefaults defaults, ServerConfig config, ServerListenerLoop listenerLoop)
{
    private Thread? thread;
    private Action? stop;

    public void Start()
    {
        if (!(config.EnableRawTcp ?? defaults.EnableRawTcp))
            return;

        int port = 36677;

        (thread, stop) = listenerLoop.Run(port, (tcp) => new(log, tcp, tcp.GetStream()));
        thread.Start();

        log.Info("Listening on port {0}...", port);
    }

    public void Stop() => stop?.Invoke();

    public void Join()
    {
        thread?.Join();
        log.Info("Listener stopped");
    }
}
