namespace Craftdig.World.Backend;

[WorldLoader]
public class WorldBackendUnloader(WorldEntRegionThread entRegionThread)
{
    public void Run()
    {
        entRegionThread.Stop();
    }
}
