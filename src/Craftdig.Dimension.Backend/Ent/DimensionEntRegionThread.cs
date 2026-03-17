namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionEntRegionThread(
    DimensionEntRegionFlusherThreads flusherThreads,
    DimensionEntRegionFileHandles fileHandles)
{
    private Timer? timer;
    private bool stop;

    public void Start()
    {
        timer = new((x) => Tick(), null, 0, 500);
        flusherThreads.Start();
    }

    public void Stop()
    {
        lock (this)
        {
            timer?.Dispose();
            stop = true;
            fileHandles.Drain();
            flusherThreads.Stop();
        }
    }

    private void Tick()
    {
        lock (this)
        {
            if (stop)
                return;

            fileHandles.Flush();
        }
    }
}
