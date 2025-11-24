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
        try
        {
            log.Info($"Socket connected");
            loop.Run(socket);
        }
        catch (Exception e)
        {
            if (socket.Connected)
                log.Warn("Error in socket", e);
        }
        finally
        {
            socket.Disconnect();
            log.Info($"Socket disconnected");

            sockets.Remove(socket);
        }
    }
}
