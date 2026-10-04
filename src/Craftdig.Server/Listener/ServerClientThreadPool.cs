namespace Craftdig;

[Server]
public class ServerClientThreadPool(Log log)
{
    private readonly Lock gate = new();
    private readonly Stack<ClientThread> pool = [];
    private readonly List<ClientThread> workers = [];
    private bool stop;

    public ClientThreadExecution Start(Action<ClientThreadExecution> action)
    {
        lock (gate)
        {
            var worker = pool.Count == 0 ? Create() : pool.Pop();
            var execution = new ClientThreadExecution(worker, worker.CurrentExecutionId);
            worker.Action = action;
            worker.Semaphore.Release();
            return execution;
        }
    }

    public void StopAndJoin()
    {
        ClientThread[] active;

        lock (gate)
        {
            stop = true;
            active = workers.ToArray();

            while (pool.TryPop(out var worker))
                worker.Semaphore.Release();
        }

        foreach (var worker in active)
            worker.Worker.Join();

        log.Info("Client threads stopped");
    }

    private ClientThread Create()
    {
        var worker = new ClientThread();
        worker.Worker = new Thread(() => Loop(worker));
        workers.Add(worker);
        worker.Worker.Start();
        return worker;
    }

    private void Loop(ClientThread worker)
    {
        while (true)
        {
            worker.Semaphore.Wait();

            if (worker.Action == null)
                break;

            worker.Action(new(worker, worker.CurrentExecutionId));
            worker.Action = null;
            worker.Complete();

            lock (gate)
            {
                if (stop || pool.Count >= 32)
                    break;

                pool.Push(worker);
            }
        }

        worker.Semaphore.Dispose();

        lock (gate)
            workers.Remove(worker);
    }
}
