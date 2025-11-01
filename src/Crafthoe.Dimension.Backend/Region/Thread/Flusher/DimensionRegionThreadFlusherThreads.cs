namespace Crafthoe.Dimension.Backend;

[Dimension]
public class DimensionRegionThreadFlusherThreads(
    DimensionRegionThreadFlusherBag bag,
    DimensionRegionThreadFlusherWorker worker)
{
    private readonly List<Thread> threads = [];
    private bool stop;

    public void Start()
    {
        for (int i = 0; i < 64; i++)
        {
            var t = new Thread(Loop);
            t.Start();
            threads.Add(t);
        }
    }

    public void Stop()
    {
        stop = true;
        bag.Release(ushort.MaxValue);

        foreach (var t in threads)
            t.Join();
    }

    private void Loop()
    {
        while (true)
        {
            if (!stop)
                bag.WaitConsumer();

            if (stop && bag.Count == 0)
                break;

            if (bag.TryTake(out var op))
            {
                worker.Work(op);
                bag.SignalProducer();
            }
        }
    }
}
