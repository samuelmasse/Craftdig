namespace Craftdig;

[DimensionLoader]
public class DimensionServerLoader(
    DimensionEntIdxContext context,
    DimensionScratchedBag scratchedBag,
    DimensionEntScratched scratched,
    DimensionServerEntTracker entTracker,
    DimensionServerEntDisposeTracker entDisposeTracker)
{
    public void Run()
    {
        context.AddBag(scratchedBag);
        context.OnWrite<bool, WorldComponents.IsLoaded>(scratched.Mark);
        context.OnWrite<bool, WorldBackendComponents.IsLoading>(scratched.Mark);
        context.OnClearing(entDisposeTracker.Capture);
        context.OnDisposing(entDisposeTracker.Capture);
        entTracker.Run();
    }
}
