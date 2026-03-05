namespace Craftdig.Server;

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
            ns.ConnectedTime = DateTime.UtcNow;
            ns.Tag = $"s{++nextSocketId}";

            clientThreadPool.Start((execution) => Loop(execution, ns));
            clientThreadPool.Start((execution) => Push(execution, ns));
        }
    }

    private void Loop(ClientThreadExecution execution, NetSocket ns)
    {
        sockets.Add(ns);
        clientLimits.Pulse();
        ns.SocketThread = execution;

        try
        {
            log.Debug("Socket {0} loop running on thread {1}", ns.Tag, execution.ClientThread.Id);
            log.Info("Socket {0} connected : {1}", ns.Tag, ns.Ip);
            loop.Run(ns);
        }
        catch (Exception e)
        {
            if (ns.Connected)
                log.Warn("Socket {0} crashed", ns.Tag, e);
        }
        finally
        {
            ns.Disconnect();
            log.Info("Socket {0} disconnected", ns.Tag);

            sockets.Remove(ns);
            clientLimits.Pulse();
        }
    }

    private void Push(ClientThreadExecution execution, NetSocket ns)
    {
        try
        {
            log.Debug("Socket {0} push running on thread {1}", ns.Tag, execution.ClientThread.Id);
            ns.Push(default);
        }
        catch (Exception e)
        {
            if (ns.Connected)
                log.Warn("Socket {0} push crashed", ns.Tag, e);
        }
        finally
        {
            ns.Disconnect();
        }
    }
}
