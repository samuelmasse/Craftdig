namespace Craftdig.World.Backend;

[World]
public class WorldBackend(WorldEntTracker entTracker, WorldEntPersister entPersister)
{
    public void Tick()
    {
        entTracker.Tick();
    }

    public void Frame()
    {
        entPersister.Frame();
    }
}
