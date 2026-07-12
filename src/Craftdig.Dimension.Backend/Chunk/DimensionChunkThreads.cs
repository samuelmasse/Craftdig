namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionChunkThreads(
    DimensionChunkThreadWorkQueue queue,
    DimensionChunkThreadOutputBag output,
    DimensionChunkThreadWorker worker)
{
    private readonly List<Thread> threads = [];
    private bool stop;

    public void Start()
    {
        int count = Math.Clamp((Environment.ProcessorCount - 1) / 2, 1, 8);
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

        foreach (var t in threads)
            t.Join();

        while (output.TryTake(out var item))
            item.Light.Clear();
    }

    private void Loop()
    {
        while (true)
        {
            queue.Wait();

            if (stop)
                break;

            if (queue.TryDequeue(out var input))
                worker.Work(input);
        }
    }
}
