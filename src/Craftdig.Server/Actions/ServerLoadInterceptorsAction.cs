namespace Craftdig.Server;

[Server]
public class ServerLoadInterceptorsAction(WorldEntIdxContextBuilder context, ServerScratchedBagMut scratchedBag)
{
    public void Run()
    {
        context.AddBagUnloaded(scratchedBag);
    }
}
