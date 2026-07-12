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
    DimensionEntDisposeTracker entDisposeTracker,
    DimensionLighting lighting)
{
    public void Run()
    {
        context.AddPreDispose(entDisposeTracker.InterceptDispose);
        context.AddPost<Vec3d, DimensionComponents.Position>(playerSync.Intercept);
        context.AddPost<bool, DimensionComponents.IsPlayer>(playerSync.Intercept);
        indexedComponents.AddSaved<DimensionComponents>();

        lighting.Start();

        chunkThreads.Start();
        regionThread.Start();
        entRegionThread.Start();
        entTracker.Tick();
    }
}
