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
            flusherQueue.Flush((handle, false));
        }

        flusherQueue.WaitAll();

        Console.WriteLine($"{handles.Count} {(DateTime.UtcNow - now).TotalMilliseconds}");
    }

    public void Drain()
    {
        var now = DateTime.UtcNow;

        while (queue.Count > 0)
            set.Remove(queue.Dequeue().Handle);

        foreach (var handle in handles.Values)
            flusherQueue.Flush((handle, true));

        flusherQueue.WaitAll();
        handles.Clear();

        Console.Write($"Drain took {(DateTime.UtcNow - now).TotalMilliseconds}ms");
    }
}
