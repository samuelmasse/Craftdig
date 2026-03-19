namespace Craftdig.World.Backend;

[World]
public class WorldEntPersister(WorldEntRegionWriter entRegionWriter)
{
    private readonly PriorityQueue<Persistence, DateTime> pq = new();
    private readonly Random rng = new();

    public void Frame()
    {
        var now = DateTime.UtcNow;

        while (pq.Count > 0 && pq.Peek().Time < now)
        {
            var (ent, _) = pq.Dequeue();
            Write(ent);
        }
    }

    public void Drain()
    {
        while (pq.Count > 0)
        {
            var (ent, _) = pq.Dequeue();
            Write(ent);
        }
    }

    public void Schedule(EntMutIdx ent)
    {
        var time = DateTime.UtcNow + TimeSpan.FromMilliseconds(rng.Next(500, 750));
        pq.Enqueue(new(ent, time), time);
    }

    public void Erase(EntMutIdx ent)
    {
        entRegionWriter.Erase(ent);
    }

    private void Write(EntMutIdx ent)
    {
        if (!ent.IsAlive)
            return;

        entRegionWriter.Write(ent);

        ent.IsDirty = false;

        var dirty = ent.DirtyComponents;
        if (dirty != null)
            Array.Clear(dirty);
    }

    private readonly record struct Persistence(EntMutIdx Ent, DateTime Time);
}
