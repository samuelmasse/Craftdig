namespace Craftdig;

[WorldLoader]
public class WorldServerLoader(
    WorldEntIdxContext context,
    WorldScratchedBag scratchedBag,
    WorldEntScratched scratched,
    WorldServerEntTracker entTracker,
    WorldServerEntDisposeTracker entDisposeTracker)
{
    public void Run()
    {
        context.AddBag(scratchedBag);
        context.OnWrite<bool, WorldBackendComponents.IsLoading>(scratched.Mark);
        context.OnClearing(entDisposeTracker.Capture);
        context.OnDisposing(entDisposeTracker.Capture);
        entTracker.Run();
    }
}
