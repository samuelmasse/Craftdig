namespace Crafthoe.Server;

[Server]
public class ServerClientLimits(AppLog log, ServerSockets sockets)
{
    private readonly ManualResetEventSlim gate = new(true);
    private readonly List<NetSocket> buffer = [];
    private bool stop;

    public void Pulse()
    {
        lock (this)
        {
            if (stop)
                return;

            sockets.ForEach(buffer.Add);

            int unauthCount = 0;
            foreach (var socket in buffer)
            {
                if (!socket.Ent.IsAuthenticated())
                    unauthCount++;
            }

            if (unauthCount > 15)
            {
                if (gate.IsSet)
                {
                    log.Warn("Client limit gate turn on : {0}", unauthCount);
                    gate.Reset();
                }
            }
            else
            {
                if (!gate.IsSet)
                {
                    log.Warn("Client limit gate turned off : {0}", unauthCount);
                    gate.Set();
                }
            }

            buffer.Clear();
        }
    }

    public void Stop()
    {
        lock (this)
        {
            stop = true;
            gate.Set();
        }
    }

    public void Wait()
    {
        gate.Wait();
    }
}
