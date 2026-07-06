namespace Craftdig.Server.Backend;

[World]
public class WorldServerEntDisposeTracker()
{
    public void InterceptDispose(EntMutIdx ent)
    {
        _ = ent;
        // TODO: do something when ent is disposed
    }
}
