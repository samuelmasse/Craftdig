namespace Craftdig.World.Backend;

[WorldLoader]
public class WorldBackendLoader(
    AppLog log,
    WorldEntRegionStates entRegionStates,
    WorldEntIdxContextBuilder context,
    WorldEntIndex entIndex,
    WorldIndexedComponentsMut indexedComponents,
    WorldEntTracker entTracker,
    WorldEntRegionThread entRegionThread,
    WorldModuleIndicesLoader moduleIndicesLoader,
    WorldEntDisposeTracker entDisposeTracker)
{
    public void Run()
    {
        moduleIndicesLoader.Run();

        context.AddPreDispose(entDisposeTracker.InterceptDispose);
        context.AddPre<Guid, WorldComponents.Id>(entIndex.Intercept);
        indexedComponents.AddSaved<WorldComponents>();

        entTracker.Tick();
        entRegionThread.Start();

        var region = entRegionStates[default];
        log.Info("Loaded {0} world ents", region.Ents.Count);
    }
}
