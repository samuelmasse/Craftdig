namespace Craftdig;

public abstract class EntScratched
{
    public void Mark(EntMutIdx ent)
    {
        if (!ent.IsLoading && !ent.IsScratched)
            ent.IsScratched = true;
    }

    public void Mark(EntMutIdx ent, int index)
    {
        if (ent.IsLoading)
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

        if (!ent.IsScratched)
            ent.IsScratched = true;
    }
}

[World]
public class WorldEntScratched : EntScratched;
