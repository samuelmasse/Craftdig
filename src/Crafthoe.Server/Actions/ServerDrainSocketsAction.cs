namespace Crafthoe.Server;

[Server]
public class ServerDrainSocketsAction(ServerSockets sockets)
{
    public void Run()
    {
        sockets.ForEach(ns => ns.Disconnect());
        sockets.ForEach(ns => ns.Ent.SocketThread()?.Join());

        Console.WriteLine("Sockets drained");
    }
}
