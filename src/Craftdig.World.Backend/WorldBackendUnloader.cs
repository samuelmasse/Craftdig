namespace Craftdig.World.Backend;

[WorldLoader]
public class WorldBackendUnloader(
    ModuleWriteWorldStateAction writeWorldStateAction,
    WorldPaths paths,
    WorldEntRegionThread entRegionThread)
{
    public void Run()
    {
        writeWorldStateAction.Write(new(DateTimeOffset.UtcNow), paths);
        entRegionThread.Stop();
    }
}
