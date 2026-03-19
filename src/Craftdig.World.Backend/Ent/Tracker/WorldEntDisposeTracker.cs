namespace Craftdig.World.Backend;

[World]
public class WorldEntDisposeTracker(WorldEntPersister entPersister)
{
    public void InterceptDispose(EntMutIdx ent)
    {
        if (ent.Ploc != null)
            entPersister.Erase(ent);
    }
}
