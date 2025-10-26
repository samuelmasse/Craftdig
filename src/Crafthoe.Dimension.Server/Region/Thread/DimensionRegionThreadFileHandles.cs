namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRegionThreadFileHandles(DimensionRegionThreadFlusherBag flusherQueue)
{
    private readonly Dictionary<string, SafeFileHandle> handles = [];
    private readonly HashSet<SafeFileHandle> set = [];
    private readonly Queue<(SafeFileHandle Handle, DateTime Time)> queue = [];

    public SafeFileHandle this[string file]
    {
        get
        {
            if (!handles.TryGetValue(file, out var handle))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file)!);

                handle = File.OpenHandle(file, FileMode.OpenOrCreate,
                    FileAccess.ReadWrite, FileShare.Read, FileOptions.RandomAccess);

                handles.Add(file, handle);
            }

            if (set.Add(handle))
                queue.Enqueue((handle, DateTime.UtcNow));

            return handle;
        }
    }

    public void Flush()
    {
        var now = DateTime.UtcNow;

        while (queue.Count > 0 && (now - queue.Peek().Time).TotalSeconds > 1)
        {
            var (handle, _) = queue.Dequeue();
            set.Remove(handle);

            if (!handle.IsClosed)
                flusherQueue.Flush((handle, false));
        }

        flusherQueue.WaitAll();
    }

    public void Drain()
    {
        while (queue.Count > 0)
            set.Remove(queue.Dequeue().Handle);

        foreach (var handle in handles.Values)
            flusherQueue.Flush((handle, true));

        flusherQueue.WaitAll();
        handles.Clear();
    }

    public void Drain(ReadOnlySpan<string> files)
    {
        foreach (var file in files)
        {
            if (handles.TryGetValue(file, out var handle))
                flusherQueue.Flush((handle, true));
        }

        flusherQueue.WaitAll();

        foreach (var file in files)
            handles.Remove(file);
    }
}
