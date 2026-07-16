namespace Craftdig.Dimension.Backend;

[DimensionLoader]
public class DimensionBackendLoader(
    DimensionIndexedComponentsMut indexedComponents,
    DimensionEntIdxContextBuilder context,
    DimensionPlayerSync playerSync,
    DimensionPlayerIndex playerIndex,
    DimensionChunkThreads chunkThreads,
    DimensionRegionThread regionThread,
    DimensionEntRegionThread entRegionThread,
    DimensionEntTracker entTracker,
    DimensionEntDisposeTracker entDisposeTracker)
{
    public void Run()
    {
        context.AddPreDispose(entDisposeTracker.InterceptDispose);
        context.AddPreDispose(playerIndex.InterceptDispose);
        context.AddPost<EntMutIdx, DimensionComponents.WorldPlayer>(playerIndex.Intercept);
        context.AddPost<Vec3d, DimensionComponents.Position>(playerSync.Intercept);
        context.AddPost<bool, DimensionComponents.IsPlayer>(playerSync.Intercept);
        indexedComponents.AddSaved<DimensionComponents>();

        chunkThreads.Start();
        regionThread.Start();
        entRegionThread.Start();
        entTracker.Tick();
    }
}
