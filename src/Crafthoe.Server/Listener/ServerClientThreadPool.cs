namespace Crafthoe.Server;

[Server]
public class ServerClientThreadPool(AppLog log)
{
    private readonly ConcurrentBag<ClientThread> pool = [];

    public void Start(Action<ClientThreadExecution> action)
    {
        if (!pool.TryTake(out var thread))
            thread = Create();

        thread.Action = action;
        thread.Semaphore.Release();
    }

    public void Stop()
    {
        while (!pool.IsEmpty)
        {
            if (pool.TryTake(out var thread))
            {
                thread.Stop = true;
                thread.Semaphore.Release();
            }
        }
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
        log.Trace("Client thread {0} started", thread.Id);

        while (true)
        {
            log.Trace("Client thread {0} waiting", thread.Id);
            thread.Semaphore.Wait();
            if (thread.Stop)
            {
                log.Trace("Client thread {0} stopped", thread.Id);
                return;
            }

            log.Trace("Client thread {0} running execution {1}", thread.Id, thread.CurrentExecutionId);
            thread.Action?.Invoke(new(thread, thread.CurrentExecutionId));
            thread.Action = null;
            thread.CurrentExecutionId++;

            if (pool.Count < 32)
            {
                log.Trace("Client thread {0} returning to pool", thread.Id);
                pool.Add(thread);
            }
            else
            {
                log.Trace("Client thread {0} dropped", thread.Id);
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
