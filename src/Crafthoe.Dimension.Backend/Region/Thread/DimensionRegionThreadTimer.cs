namespace Crafthoe.Dimension.Backend;

[Dimension]
public class DimensionRegionThreadTimer(
    DimensionRegionThreadWorkQueue queue,
    DimensionRegionThreadFileHandles fileHandles)
{
    private Timer? timer;

    public void Start()
    {
        timer = new((x) => Tick(), null, 0, 500);
    }

    public void Stop()
    {
        if (timer == null)
            return;

        timer.Dispose();
        fileHandles.Drain();
    }

    private void Tick()
    {
        queue.Enqeue(new(default, RegionThreadInputType.Flush, default!, default));
    }
}
