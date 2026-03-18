namespace Craftdig.World.Backend;

[World]
public class WorldEntRegionFlusherWorker()
{
    public void Work((SafeFileHandle, bool) op)
    {
        var (handle, dispose) = op;

        RandomAccess.FlushToDisk(handle);
        if (dispose)
            handle.Dispose();
    }
}
