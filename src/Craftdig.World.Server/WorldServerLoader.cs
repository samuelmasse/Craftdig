namespace Craftdig;

[WorldLoader]
public class WorldServerLoader(
    WorldEntIdxContextBuilder context,
    WorldScratchedBagMut scratchedBag,
    WorldEntScratched scratched,
    WorldServerEntTracker entTracker,
    WorldServerEntDisposeTracker entDisposeTracker)
{
    public void Run()
    {
        context.AddBag(scratchedBag);
        context.AddPost<bool, WorldBackendComponents.IsLoading>(scratched.Mark);
        context.AddPreDispose(entDisposeTracker.InterceptDispose);
        entTracker.Run();
    }
}
