namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionEntDisposals : EntDisposals
{
    private readonly Queue<DimensionEntDisposal> entries = [];

    public void Add(EntMutIdx ent, NetSocket? owner)
    {
        Begin(ent);
        var id = ent.SyncCloc == null ? Guid.Empty : ent.Id;
        entries.Enqueue(new(ent, id, ent.SyncCloc, owner));
    }

    public bool TryTake(out DimensionEntDisposal entry)
    {
        if (!entries.TryDequeue(out entry))
            return false;

        Finish(entry.Ent);
        return true;
    }
}
