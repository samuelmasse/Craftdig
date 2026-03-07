namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionEntityPersister(AppLog log, DimensionEntityRegionWriter entityRegionWriter)
{
    private readonly PriorityQueue<Persistance, DateTime> pq = new();
    private readonly Random rng = new();

    public void Frame()
    {
        var now = DateTime.UtcNow;
        var pass = now - TimeSpan.FromMilliseconds(500);

        while (pq.Count > 0 && pq.Peek().Time < pass)
        {
            var (ent, rloc, prevRloc, _, persistId) = pq.Dequeue();
            if (persistId != ent.RigidPersistId)
                continue;

            log.Warn("Persist ent {0} into rloc {1}", ent, prevRloc);
            entityRegionWriter.Write(ent, rloc);

            if (prevRloc != null && prevRloc != rloc)
            {
                log.Warn("Remove ent {0} from rloc {1}", ent, prevRloc);
                entityRegionWriter.Erase(ent, prevRloc.Value);
            }

            var nextTime = now + TimeSpan.FromMilliseconds(rng.Next(250, 500));
            pq.Enqueue(new(ent, rloc, rloc, nextTime, persistId), nextTime);
        }
    }

    public void Schedule(Ent ent, Vector2i rloc, DateTime time)
    {
        var prevRloc = ent.RigidRloc;

        log.Warn("Persisting {0} {1} {2} {3} {4}", ent, rloc, prevRloc, time, ent.RigidPersistId);
        pq.Enqueue(new(ent, rloc, prevRloc, time, ent.RigidPersistId), time);
    }

    private readonly record struct Persistance(Ent Ent, Vector2i Rloc, Vector2i? PrevRloc, DateTime Time, long PersistId);
}
