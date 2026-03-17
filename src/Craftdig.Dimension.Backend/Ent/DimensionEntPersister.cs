namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionEntPersister(AppLog log, DimensionEntRegionWriter entRegionWriter)
{
    private readonly PriorityQueue<Persistence, DateTime> pq = new();
    private readonly Random rng = new();

    public void Frame()
    {
        var now = DateTime.UtcNow;

        while (pq.Count > 0 && pq.Peek().Time < now)
        {
            var (ent, _) = pq.Dequeue();

            log.Warn("Persist ent {0}", ent);
            entRegionWriter.Write(ent);

            ent.IsDirty = false;

            var dirty = ent.DirtyComponents;
            if (dirty != null)
                Array.Clear(dirty);
        }
    }

    public void Schedule(EntMutIdx ent)
    {
        var time = DateTime.UtcNow + TimeSpan.FromMilliseconds(rng.Next(500, 750));

        log.Warn("Persisting {0} {1}", ent, time);
        pq.Enqueue(new(ent, time), time);
    }

    private readonly record struct Persistence(EntMutIdx Ent, DateTime Time);
}
