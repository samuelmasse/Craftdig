namespace Crafthoe.Dimension;

[Dimension]
public class DimensionSectionThreads(
    DimensionSectionThreadWorkQueue queue,
    DimensionSectionThreadWorker worker)
{
    private readonly List<Thread> threads = [];
    private bool stop;

    public void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            var t = new Thread(Loop);
            t.Start();
            threads.Add(t);
        }
    }

    public void Stop()
    {
        stop = true;
        queue.Release(threads.Count);

        foreach (var t in threads)
            t.Join();
    }

    private void Loop()
    {
        while (true)
        {
            queue.Wait();
            if (stop)
                break;

            if (queue.TryDequeue(out var sloc))
                worker.Work(sloc);
        }
    }
}
