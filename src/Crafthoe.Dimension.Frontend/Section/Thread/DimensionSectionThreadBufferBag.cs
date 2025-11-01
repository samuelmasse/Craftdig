namespace Crafthoe.Dimension.Frontend;

[Dimension]
public class DimensionSectionThreadBufferBag
{
    private readonly ConcurrentBag<List<BlockVertex>> bag = [];
    private readonly SemaphoreSlim semaphore = new(64);

    public void Release(int count) => semaphore.Release(count);

    public void Add(List<BlockVertex> buffer)
    {
        bag.Add(buffer);
        semaphore.Release();
    }

    public List<BlockVertex> Take()
    {
        semaphore.Wait();

        if (bag.TryTake(out var buffer))
            return buffer;

        return [];
    }
}
