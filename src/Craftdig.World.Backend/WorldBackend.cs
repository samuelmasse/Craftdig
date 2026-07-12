namespace Craftdig.World.Backend;

[World]
public class WorldBackend(WorldClock clock, WorldEntTracker entTracker, WorldEntPersister entPersister)
{
    public void Tick()
    {
        clock.Tick();
        entTracker.Tick();
    }

    public void Frame()
    {
        entPersister.Frame();
    }
}
