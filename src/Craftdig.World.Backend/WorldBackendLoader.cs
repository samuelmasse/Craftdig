namespace Craftdig.World.Backend;

[WorldLoader]
public class WorldBackendLoader(
    Log log,
    ModuleWriteWorldStateAction writeWorldStateAction,
    WorldPaths paths,
    WorldEntRegionStates entRegionStates,
    WorldEntIdxContextBuilder context,
    WorldEntIndex entIndex,
    WorldIndexedComponentsMut indexedComponents,
    WorldUniverseLoader universeLoader,
    WorldTimeLoader timeLoader,
    WorldEntTracker entTracker,
    WorldEntRegionThread entRegionThread,
    WorldModuleIndicesLoader moduleIndicesLoader,
    WorldEntDisposeTracker entDisposeTracker)
{
    public void Run()
    {
        writeWorldStateAction.Write(new(DateTimeOffset.UtcNow), paths);
        moduleIndicesLoader.Run();

        context.AddPreDispose(entDisposeTracker.InterceptDispose);
        context.AddPreDispose(entIndex.InterceptDispose);
        context.AddPre<Guid, WorldComponents.Id>(entIndex.Intercept);
        indexedComponents.AddSaved<WorldComponents>();

        entTracker.Tick();
        entRegionThread.Start();

        var region = entRegionStates[default];
        universeLoader.Run(region);
        timeLoader.Run();
        log.Info("Loaded {0} world ents", region.Ents.Count);
    }
}
