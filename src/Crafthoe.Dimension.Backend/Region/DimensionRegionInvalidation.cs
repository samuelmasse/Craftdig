namespace Crafthoe.Dimension.Backend;

[Dimension]
public class DimensionRegionInvalidation(
    DimensionBlockChanges blockChanges,
    DimensionBlocksRaw blocksRaw,
    DimensionRegionThreadWorkQueue regionThreadWorkQueue)
{
    private readonly Dictionary<Vector3i, DateTime> dirty = [];
    private readonly HashSet<Vector3i> scheduled = [];

    public void Frame()
    {
        var now = DateTime.UtcNow;

        foreach (var c in blockChanges.Span)
        {
            var sloc = c.Loc.ToSloc();
            dirty.TryAdd(sloc, now);
        }

        foreach (var d in dirty)
        {
            if ((now - d.Value).TotalMilliseconds > 100)
                scheduled.Add(d.Key);
        }

        foreach (var sloc in scheduled)
        {
            Write(sloc);
            dirty.Remove(sloc);
        }

        scheduled.Clear();
    }

    public void Drain()
    {
        foreach (var d in dirty)
            Write(d.Key);

        dirty.Clear();
    }

    public void Drain(Vector3i sloc)
    {
        if (dirty.Remove(sloc))
            Write(sloc);
    }

    private void Write(Vector3i sloc)
    {
        if (!blocksRaw.TryGetChunkBlocks(sloc.Xy, out var blocks))
            return;

        regionThreadWorkQueue.Enqeue(new(sloc, RegionThreadInputType.WriteSection, blocks, sloc.Z));
    }
}
