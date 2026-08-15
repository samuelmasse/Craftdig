namespace Craftdig;

[Dimension]
public class DimensionSectionThreadWorkQueue
{
    private readonly SemaphoreSlim semaphore = new(0);
    private readonly ConcurrentQueue<SectionThreadInput> q = [];

    public int Count => q.Count;

    public void Enqueue(SectionThreadInput input)
    {
        q.Enqueue(input);
        semaphore.Release();
    }

    public void Release(int count) => semaphore.Release(count);

    public void Wait() => semaphore.Wait();
    public bool TryDequeue(out SectionThreadInput input) => q.TryDequeue(out input);
}
