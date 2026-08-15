namespace Craftdig;

public abstract class EntDisposals
{
    private readonly HashSet<EntMutIdx> disposing = [];

    public bool Contains(EntMutIdx ent) => disposing.Contains(ent);

    protected void Begin(EntMutIdx ent) => disposing.Add(ent);

    protected void Finish(EntMutIdx ent) => disposing.Remove(ent);
}

public readonly record struct WorldEntDisposal(EntMutIdx Ent, Guid Id);

[World]
public class WorldEntDisposals : EntDisposals
{
    private readonly Queue<WorldEntDisposal> entries = [];

    public void Add(EntMutIdx ent)
    {
        Begin(ent);
        entries.Enqueue(new(ent, ent.IsWorldSyncPublished ? ent.Id : Guid.Empty));
    }

    public bool TryTake(out WorldEntDisposal entry)
    {
        if (!entries.TryDequeue(out entry))
            return false;

        Finish(entry.Ent);
        return true;
    }
}
