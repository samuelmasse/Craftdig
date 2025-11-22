namespace Crafthoe.Server;

[Server]
public class ServerDrainSocketsAction(AppLog log, ServerSockets sockets)
{
    public void Run()
    {
        sockets.ForEach(ns => ns.Disconnect());
        sockets.ForEach(ns => ns.Ent.SocketThread()?.Join());

        log.Info("Sockets drained");
    }
}
