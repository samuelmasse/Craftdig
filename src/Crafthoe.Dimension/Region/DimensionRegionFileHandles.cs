namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRegionFileHandles
{
    private readonly Dictionary<string, SafeFileHandle> handles = [];
    private readonly HashSet<SafeFileHandle> set = [];
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
            set.Remove(handle);

        var now = DateTime.UtcNow;

        while (queue.Count > 0 && (now - queue.Peek().Time).TotalSeconds > 1)
        {
            var (handle, _) = queue.Dequeue();

            Task.Run(() =>
            {
                RandomAccess.FlushToDisk(handle);
                flushed.Enqueue(handle);
            });
        }
    }
}
