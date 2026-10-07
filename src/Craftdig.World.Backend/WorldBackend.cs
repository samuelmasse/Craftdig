namespace Craftdig;

[World]
public class WorldBackend(WorldClock clock, WorldEntPersister entPersister)
{
    public void Tick()
    {
        clock.Tick();
    }

    public void Frame()
    {
        entPersister.Frame();
    }
}
