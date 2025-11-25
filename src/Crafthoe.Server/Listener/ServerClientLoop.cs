namespace Crafthoe.Server;

[Server]
public class ServerClientLoop(
    AppLog log,
    ServerNetLoop loop,
    ServerSockets sockets,
    ServerClientThreadPool clientThreadPool,
    ServerClientLimits clientLimits)
{
    private long nextSocketId;

    public void Start(NetSocket ns)
    {
        lock (this)
        {
            ns.MaxMessageSize = 4096;
            ns.Ent.ConnectedTime() = DateTime.UtcNow;
            ns.Ent.Tag() = $"s{++nextSocketId}";

            clientThreadPool.Start((execution) => Loop(execution, ns));
        }
    }

    private void Loop(ClientThreadExecution execution, NetSocket ns)
    {
        sockets.Add(ns);
        clientLimits.Pulse();
        ns.Ent.SocketThread() = execution;

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
            clientLimits.Pulse();
        }
    }
}
