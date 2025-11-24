namespace Crafthoe.Server;

[Server]
public class ServerClientLoop(AppLog log, ServerNetLoop loop, ServerSockets sockets)
{
    private long nextSocketId;

    public void Start(NetSocket ns)
    {
        lock (this)
        {
            var thread = new Thread(() => Loop(ns));
            ns.Ent.SocketThread() = thread;
            ns.Ent.Tag() = $"s{++nextSocketId}";
            thread.Start();
            sockets.Add(ns);
        }
    }

    private void Loop(NetSocket ns)
    {
        try
        {
            log.Info("Socket {0} connected : {1}", ns.Ent.Tag(), ns.Ip);
            loop.Run(ns);
        }
        catch (Exception e)
        {
            if (ns.Connected)
                log.Warn("Socket {0} crashed", ns.Ent.Tag(), e);
        }
        finally
        {
            ns.Disconnect();
            log.Info("Socket {0} disconnected", ns.Ent.Tag());

            sockets.Remove(ns);
        }
    }
}
