namespace Craftdig;

[DimensionLoader]
public class DimensionBackendLoader(
    DimensionIndexedComponentsMut indexedComponents,
    DimensionEntIdxContext context,
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
        context.OnClearing(entDisposeTracker.Erase);
        context.OnDisposing(entDisposeTracker.Erase);
        context.AddIndex(playerIndex.Remove)
            .OnChange<Guid, WorldComponents.Id>(playerIndex.UpdateId)
            .OnChange<bool, DimensionComponents.IsPlayer>(playerIndex.UpdatePlayer);
        context.OnWrite<Vec3d, DimensionComponents.Position>(playerSync.Update);
        context.OnWrite<bool, DimensionComponents.IsPlayer>(playerSync.Update);
        context.OnWrite<bool, WorldBackendComponents.IsLoading>(playerSync.Update);
        context.OnWrite<EntMutIdx, DimensionComponents.WorldPlayer>(playerSync.Update);
        indexedComponents.AddSaved<DimensionComponents>();

        chunkThreads.Start();
        regionThread.Start();
        entRegionThread.Start();
        entTracker.Register();
    }
}
