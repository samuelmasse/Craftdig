namespace Craftdig;

[World]
public class WorldServerEntDisposeTracker(WorldEntDisposals disposals)
{
    public void InterceptDispose(EntMutIdx ent)
    {
        disposals.Add(ent);
    }
}
