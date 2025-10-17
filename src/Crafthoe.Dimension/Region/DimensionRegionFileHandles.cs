namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRegionFileHandles
{
    private readonly Dictionary<string, SafeFileHandle> handles = [];
    private readonly HashSet<SafeFileHandle> set = [];
    private readonly HashSet<SafeFileHandle> pending = [];
    private readonly Queue<(SafeFileHandle Handle, DateTime Time)> queue = [];
    private readonly ConcurrentQueue<SafeFileHandle> flushed = [];

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
        while (flushed.TryDequeue(out var handle))
            pending.Remove(handle);

        var now = DateTime.UtcNow;

        while (queue.Count > 0 &&
            (now - queue.Peek().Time).TotalSeconds > 1 &&
            !pending.Contains(queue.Peek().Handle))
        {
            var (handle, _) = queue.Dequeue();
            set.Remove(handle);

            Task.Run(() =>
            {
                RandomAccess.FlushToDisk(handle);
                flushed.Enqueue(handle);
            });
        }
    }

    public void Drain()
    {
        while (queue.Count > 0)
            set.Remove(queue.Dequeue().Handle);

        while (pending.Count > 0)
        {
            while (flushed.TryDequeue(out var handle))
                pending.Remove(handle);
        }

        foreach (var handle in handles.Values)
        {
            RandomAccess.FlushToDisk(handle);
            handle.Dispose();
        }

        handles.Clear();
    }
}
