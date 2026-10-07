namespace Craftdig;

[World]
public class WorldServerEntDisposeTracker(WorldEntDisposals disposals)
{
    public void Capture(EntMutIdx ent)
    {
        disposals.Add(ent);
    }
}
