namespace Craftdig;

[World]
public class WorldEntDisposals
{
    private readonly Queue<Guid> entries = [];

    public void Add(EntMutIdx ent)
    {
        if (ent.IsWorldSyncPublished && ent.Id != Guid.Empty)
            entries.Enqueue(ent.Id);
    }

    public bool TryTake(out Guid id) => entries.TryDequeue(out id);
}
