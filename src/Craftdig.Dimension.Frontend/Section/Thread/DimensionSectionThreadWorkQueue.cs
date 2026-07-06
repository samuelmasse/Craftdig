namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionSectionThreadWorkQueue
{
    private readonly SemaphoreSlim semaphore = new(0);
    private readonly ConcurrentQueue<Vec3i> q = [];

    public void Enqeue(Vec3i sloc)
    {
        q.Enqueue(sloc);
        semaphore.Release();
    }

    public void Release(int count) => semaphore.Release(count);

    public void Wait() => semaphore.Wait();
    public bool TryDequeue(out Vec3i sloc) => q.TryDequeue(out sloc);
}
