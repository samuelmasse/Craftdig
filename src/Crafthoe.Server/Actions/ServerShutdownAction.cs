namespace Craftdig.Server;

[Server]
public class ServerShutdownAction(
    ServerTicks ticks,
    ServerListener listener,
    ServerListenerTls listenerTls,
    ServerClientLimits clientLimits)
{
    public void Run()
    {
        listener.Stop();
        listenerTls.Stop();
        clientLimits.Stop();
        ticks.Stop();
    }
}
