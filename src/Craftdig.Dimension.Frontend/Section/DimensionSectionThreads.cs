namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionSectionThreads(
    DimensionSectionThreadWorkQueue queue,
    DimensionSectionThreadBufferBag bag,
    DimensionSectionThreadWorker worker)
{
    private readonly List<Thread> threads = [];
    private bool stop;

    public void Start()
    {
        int count = Math.Clamp((Environment.ProcessorCount - 2) / 2, 1, 8);
        for (int i = 0; i < count; i++)
        {
            var t = new Thread(Loop);
            t.Start();
            threads.Add(t);
        }
    }

    public void Stop()
    {
        stop = true;
        queue.Release(ushort.MaxValue);
        bag.Release(ushort.MaxValue);

        foreach (var t in threads)
            t.Join();
    }

    private void Loop()
    {
        var samples = new SectionThreadSamples();

        while (true)
        {
            queue.Wait();

            if (stop)
                break;

            if (queue.TryDequeue(out var input))
                worker.Work(input, samples);
        }
    }
}
