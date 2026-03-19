namespace Craftdig.Dimension.Backend;

[DimensionLoader]
public class DimensionBackendLoader(
    DimensionIndexedComponentsMut indexedComponents,
    DimensionEntIdxContextBuilder context,
    DimensionPlayerSync playerSync,
    DimensionChunkThreads chunkThreads,
    DimensionRegionThread regionThread,
    DimensionEntRegionThread entRegionThread,
    DimensionEntTracker entTracker,
    DimensionEntDisposeTracker entDisposeTracker)
{
    public void Run()
    {
        context.AddPreDispose(entDisposeTracker.InterceptDispose);
        context.AddPost<Vector3d, DimensionComponents.Position>(playerSync.Intercept);
        context.AddPost<bool, DimensionComponents.IsPlayer>(playerSync.Intercept);
        indexedComponents.AddSaved<DimensionComponents>();

        chunkThreads.Start();
        regionThread.Start();
        entRegionThread.Start();
        entTracker.Tick();
    }
}
