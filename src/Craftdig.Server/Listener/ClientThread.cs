namespace Craftdig;

public class ClientThread
{
    private readonly SemaphoreSlim semaphore = new(0);
    private Thread worker = null!;
    private Action<ClientThreadExecution>? action;
    private long currentExecutionId = 1;

    public int Id => worker.ManagedThreadId;
    public long CurrentExecutionId => Volatile.Read(ref currentExecutionId);
    public SemaphoreSlim Semaphore => semaphore;
    public ref Thread Worker => ref worker;
    public ref Action<ClientThreadExecution>? Action => ref action;

    public void Complete() => Interlocked.Increment(ref currentExecutionId);
}
