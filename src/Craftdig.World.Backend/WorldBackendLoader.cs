namespace Craftdig;

[WorldLoader]
public class WorldBackendLoader(
    Log log,
    ModuleWriteWorldStateAction changeWorldStateAction,
    WorldPaths paths,
    WorldEntRegionStates entRegionStates,
    WorldEntIdxContext context,
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
        changeWorldStateAction.Write(new(DateTimeOffset.UtcNow), paths);
        moduleIndicesLoader.Run();

        context.OnClearing(entDisposeTracker.Erase);
        context.OnDisposing(entDisposeTracker.Erase);
        context.AddIndex(entIndex.Remove).OnChange<Guid, WorldComponents.Id>(entIndex.Update);
        indexedComponents.AddSaved<WorldComponents>();

        entTracker.Register();
        entRegionThread.Start();

        var region = entRegionStates[default];
        universeLoader.Run(region);
        timeLoader.Run();
        log.Info("Loaded {0} world ents", region.Ents.Count);
    }
}
