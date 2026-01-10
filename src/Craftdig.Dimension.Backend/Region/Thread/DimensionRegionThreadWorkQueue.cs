namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionRegionThreadWorkQueue
{
    private readonly SemaphoreSlim semaphore = new(0);
    private readonly ConcurrentQueue<RegionThreadInput> q = [];

    public int Count => q.Count;

    public void Enqeue(RegionThreadInput input)
    {
        q.Enqueue(input);
        semaphore.Release();
    }

    public void Release(int count) => semaphore.Release(count);

    public void Wait() => semaphore.Wait();
    public bool TryDequeue(out RegionThreadInput input) => q.TryDequeue(out input);
}
