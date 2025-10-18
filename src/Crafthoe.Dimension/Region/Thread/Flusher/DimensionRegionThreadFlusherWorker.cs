namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRegionThreadFlusherWorker
{
    public void Work((SafeFileHandle, bool) op)
    {
        var (handle, dispose) = op;

        RandomAccess.FlushToDisk(handle);
        if (dispose)
            handle.Dispose();
    }
}
