namespace Crafthoe.Server;

[Server]
public class ServerClientLoop(ServerNetLoop loop, ServerSockets sockets)
{
    public void Start(NetSocket socket)
    {
        var thread = new Thread(() => Loop(socket));
        socket.Ent.SocketThread() = thread;
        thread.Start();
        sockets.Add(socket);
    }

    private void Loop(NetSocket socket)
    {
        Console.WriteLine($"Socket connected");

        try
        {
            loop.Run(socket);
        }
        catch (Exception e)
        {
            if (socket.Connected)
            {
                Console.Error.WriteLine("Error in socket");
                Console.Error.WriteLine(e);
            }
        }

        socket.Disconnect();
        Console.WriteLine($"Socket disconnected");

        sockets.Remove(socket);
    }
}
