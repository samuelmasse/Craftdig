namespace Craftdig;

[World]
public class WorldEntDisposeTracker(WorldEntPersister entPersister)
{
    public void Erase(EntMutIdx ent)
    {
        if (ent.Ploc != null)
            entPersister.Erase(ent);
    }
}
