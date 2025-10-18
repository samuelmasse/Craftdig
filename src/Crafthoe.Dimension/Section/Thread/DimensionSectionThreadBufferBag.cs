namespace Crafthoe.Dimension;

[Dimension]
public class DimensionSectionThreadBufferBag
{
    private readonly ConcurrentBag<List<BlockVertex>> bag = [];
    private readonly SemaphoreSlim semaphore = new(64);

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
