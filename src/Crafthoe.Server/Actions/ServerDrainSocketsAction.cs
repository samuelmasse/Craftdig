namespace Crafthoe.Server;

[Server]
public class ServerDrainSocketsAction(AppLog log, ServerSockets sockets, ServerClientThreadPool clientThreadPool)
{
    public void Run()
    {
        var list = new List<NetSocket>();
        sockets.ForEach(list.Add);

        list.ForEach(ns => ns.Disconnect());
        clientThreadPool.Stop();

        log.Info("Sockets drained");
    }
}
