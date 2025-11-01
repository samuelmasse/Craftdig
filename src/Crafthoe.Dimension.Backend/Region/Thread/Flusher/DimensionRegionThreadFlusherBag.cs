namespace Crafthoe.Dimension.Backend;

[Dimension]
public class DimensionRegionThreadFlusherBag
{
    private readonly SemaphoreSlim semaphore = new(0);
    private readonly CountdownEvent latch = new(0);
    private readonly ConcurrentBag<(SafeFileHandle, bool)> bag = [];

    public int Count => bag.Count;

    public void Flush((SafeFileHandle, bool) op) => bag.Add(op);
    public void WaitAll()
    {
        if (bag.IsEmpty)
            return;

        latch.Reset(bag.Count);
        semaphore.Release(bag.Count);
        latch.Wait();
    }
    public void Release(int count) => semaphore.Release(count);

    public void WaitConsumer() => semaphore.Wait();
    public bool TryTake(out (SafeFileHandle, bool) val) => bag.TryTake(out val);
    public void SignalProducer() => latch.Signal();
}
