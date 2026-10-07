namespace Craftdig;

[Dimension]
public class DimensionEntDisposals
{
    private readonly Queue<DimensionEntDisposal> entries = [];

    public void Add(EntMutIdx ent, NetSocket? owner)
    {
        var id = ent.SyncCloc == null ? Guid.Empty : ent.Id;
        entries.Enqueue(new(id, ent.SyncCloc, owner));
    }

    public bool TryTake(out DimensionEntDisposal entry) => entries.TryDequeue(out entry);
}
