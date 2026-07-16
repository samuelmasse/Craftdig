namespace Craftdig.World.Server;

public abstract class EntScratched(EntDisposals disposals)
{
    public void Mark(EntMutIdx ent)
    {
        if (!ent.IsLoading && !disposals.Contains(ent))
            ent.IsScratched = true;
    }

    public void Mark(EntMutIdx ent, int index)
    {
        if (ent.IsLoading || disposals.Contains(ent))
            return;

        int page = index / EntSyncCatalog.ComponentsPerMask;
        int sub = index % EntSyncCatalog.ComponentsPerMask;

        var scratched = ent.ScratchedComponents ??= new ulong[1];
        if (page >= scratched.Length)
        {
            var next = new ulong[page + 1];
            scratched.CopyTo(next);
            scratched = next;
            ent.ScratchedComponents = scratched;
        }

        scratched[page] |= 1UL << sub;
        ent.IsScratched = true;
    }
}

[World]
public class WorldEntScratched(WorldEntDisposals disposals) : EntScratched(disposals);
