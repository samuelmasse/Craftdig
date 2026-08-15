namespace Craftdig;

[Server]
public class ServerClientThreadPool(Log log)
{
    private readonly ConcurrentBag<ClientThread> pool = [];
    private volatile bool stop;

    public void Start(Action<ClientThreadExecution> action)
    {
        if (!pool.TryTake(out var thread))
            thread = Create();

        thread.Action = action;
        thread.Semaphore.Release();
    }

    public void Stop()
    {
        log.Debug("Stopping {0} client threads", pool.Count);

        stop = true;
        int stopped = 0;

        while (!pool.IsEmpty)
        {
            if (pool.TryTake(out var thread))
            {
                thread.Semaphore.Release();
                stopped++;
            }
        }

        log.Debug("Stopped {0} client threads", stopped);
    }

    private ClientThread Create()
    {
        var clientThread = new ClientThread();
        var thread = new Thread(() => Loop(clientThread));
        thread.Start();
        return clientThread;
    }

    private void Loop(ClientThread thread)
    {
        log.Debug("Client thread {0} started", thread.Id);

        while (true)
        {
            log.Debug("Client thread {0} waiting", thread.Id);
            if (stop)
            {
                log.Debug("Client thread {0} stopped", thread.Id);
                break;
            }
            thread.Semaphore.Wait();
            if (stop)
            {
                log.Debug("Client thread {0} stopped", thread.Id);
                break;
            }

            log.Debug("Client thread {0} running execution {1}", thread.Id, thread.CurrentExecutionId);
            thread.Action?.Invoke(new(thread, thread.CurrentExecutionId));
            thread.Action = null;
            thread.CurrentExecutionId++;

            if (!stop && pool.Count < 32)
            {
                log.Debug("Client thread {0} returning to pool", thread.Id);
                pool.Add(thread);
            }
            else
            {
                log.Debug("Client thread {0} dropped", thread.Id);
                break;
            }
        }
    }
}

public class ClientThread
{
    private static long MaxId;

    public long Id { get; } = ++MaxId;
    public SemaphoreSlim Semaphore { get; } = new(0);
    public bool Stop { get; set; }
    public long CurrentExecutionId { get; set; } = 1;
    public Action<ClientThreadExecution>? Action { get; set; }
}

public readonly record struct ClientThreadExecution(ClientThread ClientThread, long ExecutionId);
