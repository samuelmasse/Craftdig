namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionSyncChunks
{
    private readonly Dictionary<Vec2i, HashSet<EntMutIdx>> chunks = [];

    public bool TryGet(Vec2i cloc, [NotNullWhen(true)] out HashSet<EntMutIdx>? value) =>
        chunks.TryGetValue(cloc, out value);

    public void Move(EntMutIdx ent, Vec2i? previous, Vec2i current)
    {
        if (previous == current)
            return;

        if (previous != null && chunks.TryGetValue(previous.Value, out var old))
        {
            old.Remove(ent);
            if (old.Count == 0)
                chunks.Remove(previous.Value);
        }

        if (!chunks.TryGetValue(current, out var next))
        {
            next = [];
            chunks.Add(current, next);
        }

        next.Add(ent);
    }

    public void Remove(EntMutIdx ent, Vec2i? cloc)
    {
        if (cloc == null || !chunks.TryGetValue(cloc.Value, out var value))
            return;

        value.Remove(ent);
        if (value.Count == 0)
            chunks.Remove(cloc.Value);
    }
}
