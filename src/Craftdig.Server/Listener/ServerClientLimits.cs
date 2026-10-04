namespace Craftdig;

[Server]
public class ServerClientLimits(Log log, ServerSockets sockets)
{
    private readonly ManualResetEventSlim gate = new(true);
    private bool stop;

    public void Pulse()
    {
        lock (this)
        {
            if (stop)
                return;

            int unauthCount = 0;
            sockets.ForEach(socket =>
            {
                if (!socket.IsAuthenticated)
                    unauthCount++;
            });

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
