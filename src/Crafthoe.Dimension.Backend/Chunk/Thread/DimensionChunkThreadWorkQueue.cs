namespace Crafthoe.Dimension.Backend;

[Dimension]
public class DimensionChunkThreadWorkQueue
{
    private readonly SemaphoreSlim semaphore = new(0);
    private readonly ConcurrentQueue<ChunkThreadInput> q = [];

    public int Count => q.Count;

    public void Enqueue(ChunkThreadInput input)
    {
        q.Enqueue(input);
        semaphore.Release();
    }

    public void Release(int count) => semaphore.Release(count);

    public void Wait() => semaphore.Wait();
    public bool TryDequeue(out ChunkThreadInput input) => q.TryDequeue(out input);
}
