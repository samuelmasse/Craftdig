namespace Craftdig;

[World]
public class WorldEntDirty(WorldEntPersister entPersister)
{
    public void Mark(EntMutIdx ent, int index)
    {
        if (ent.IsLoading)
            return;

        int page = index / 64;
        int sub = index % 64;

        var dirty = ent.DirtyComponents ??= new ulong[1];
        if (page >= dirty.Length)
        {
            var next = new ulong[page + 1];
            dirty.CopyTo(next);
            dirty = next;
            ent.DirtyComponents = dirty;
        }

        dirty[page] |= 1UL << sub;

        if (!ent.IsDirty)
        {
            entPersister.Schedule(ent);
            ent.IsDirty = true;
        }
    }
}
