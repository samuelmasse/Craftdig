namespace Crafthoe.Server;

[Server]
public class ServerTickCheck
{
    private readonly SemaphoreSlim semaphore = new(0);

    public void Signal() => semaphore.Release();
    public void Wait() => semaphore.Wait();
}
