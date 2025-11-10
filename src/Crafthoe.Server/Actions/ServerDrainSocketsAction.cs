namespace Crafthoe.Server;

[Server]
public class ServerDrainSocketsAction(ServerSockets sockets)
{
    public void Run()
    {
        var list = sockets.ToList();

        while (list.Count > 0)
        {
            foreach (var ns in list)
                ns.Disconnect();

            foreach (var ns in list)
                ns.Ent.SocketThread()?.Join();

            list = sockets.ToList();
        }

        Console.WriteLine("Sockets drained");
    }
}
