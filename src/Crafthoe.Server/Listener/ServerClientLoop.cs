namespace Crafthoe.Server;

[Server]
public class ServerClientLoop(AppLog log, ServerNetLoop loop, ServerSockets sockets)
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
        log.Info($"Socket connected");

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
        log.Info($"Socket disconnected");

        sockets.Remove(socket);
    }
}
