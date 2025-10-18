namespace Crafthoe.Dimension;

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
        queue.Release(1);
        thread.Join();

        timer.Stop();
        flusherThreads.Stop();
    }

    private void Loop()
    {
        while (true)
        {
            if (!stop)
                queue.Wait();

            if (stop && queue.Count == 0)
                break;

            if (queue.TryDequeue(out var input))
                worker.Work(input);
        }
    }
}
