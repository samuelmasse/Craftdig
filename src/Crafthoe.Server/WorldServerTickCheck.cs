namespace Crafthoe.Server;

[World]
public class WorldServerTickCheck
{
    private readonly SemaphoreSlim semaphore = new(0);

    public void Signal() => semaphore.Release();
    public void Wait() => semaphore.Wait();
}
