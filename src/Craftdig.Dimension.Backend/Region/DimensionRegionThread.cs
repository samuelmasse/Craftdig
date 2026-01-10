namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionRegionThread(
    DimensionRegionThreadWorkQueue queue,
    DimensionRegionThreadWorker worker,
    DimensionRegionThreadTimer timer,
    DimensionRegionThreadFlusherThreads flusherThreads)
{
    private Thread? thread;
    private bool stop;

    public void Start()
    {
        thread = new(Loop);
        thread.Start();

        flusherThreads.Start();
        timer.Start();
    }

    public void Stop()
    {
        if (thread == null)
            return;

        stop = true;
        queue.Release(ushort.MaxValue);
        thread.Join();

        timer.Stop();
        flusherThreads.Stop();
    }

    private void Loop()
    {
        while (true)
        {
            queue.Wait();

            if (stop && queue.Count == 0)
                break;

            if (queue.TryDequeue(out var input))
                worker.Work(input);
        }
    }
}
