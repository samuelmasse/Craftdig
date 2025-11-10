namespace Crafthoe.Server;

[Server]
public class ServerShutdownAction(
    ServerTicks ticks,
    ServerListener listener,
    ServerListenerTls listenerTls)
{
    public void Run()
    {
        listener.Stop();
        listenerTls.Stop();
        ticks.Stop();
    }
}
