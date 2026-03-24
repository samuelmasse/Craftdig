namespace Craftdig.Server;

[Server]
public class ServerLoadInterceptorsAction(WorldEntIdxContextBuilder context, WorldScratchedBagMut scratchedBag)
{
    public void Run()
    {
        context.AddBagUnloaded(scratchedBag);
    }
}
