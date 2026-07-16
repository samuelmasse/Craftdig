namespace Craftdig.Dimension.Server;

[DimensionLoader]
public class DimensionServerLoader(
    DimensionEntIdxContextBuilder context,
    DimensionScratchedBagMut scratchedBag,
    DimensionEntScratched scratched,
    DimensionServerEntTracker entTracker,
    DimensionServerEntDisposeTracker entDisposeTracker)
{
    public void Run()
    {
        context.AddBag(scratchedBag);
        context.AddPost<bool, WorldComponents.IsLoaded>(scratched.Mark);
        context.AddPost<bool, WorldBackendComponents.IsLoading>(scratched.Mark);
        context.AddPreDispose(entDisposeTracker.InterceptDispose);
        entTracker.Run();
    }
}
