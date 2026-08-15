namespace Craftdig;

[Player]
public class PlayerAheadSections(Log log, DimensionBlockChanges blockChanges, PlayerSocket socket)
{
    private readonly Queue<(Vec3i, DateTime)> queue = [];
    private readonly Dictionary<Vec3i, DateTime> set = [];

    public void Tick()
    {
        var now = DateTime.UtcNow;

        foreach (var change in blockChanges.Span)
        {
            var sloc = change.Loc.ToSloc();
            if (set.TryAdd(sloc, now))
            {
                queue.Enqueue((sloc, now));
                log.Trace("Section {0} is ahead", sloc);
            }
        }

        while (queue.TryPeek(out var v) && (now - v.Item2).TotalSeconds > 1)
        {
            var (sloc, timeEnqueued) = queue.Dequeue();
            if (!set.TryGetValue(sloc, out var timeSet) || timeSet > timeEnqueued)
                continue;

            log.Warn("Section {0} is too much ahead", sloc);
            socket.Send<ForgetSectionCommand>(new() { Sloc = sloc });
            set.Remove(sloc);
        }
    }

    public void Remove(Vec3i sloc) => set.Remove(sloc);
}
