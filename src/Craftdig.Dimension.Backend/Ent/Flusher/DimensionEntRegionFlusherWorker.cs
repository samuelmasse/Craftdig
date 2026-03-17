namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionEntRegionFlusherWorker(AppLog log)
{
    public void Work((SafeFileHandle, bool) op)
    {
        var (handle, dispose) = op;

        RandomAccess.FlushToDisk(handle);
        if (dispose)
            handle.Dispose();
    }
}
