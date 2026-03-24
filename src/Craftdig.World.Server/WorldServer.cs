namespace Craftdig.World.Server;

[World]
public class WorldServer(
    WorldBackend backend,
    WorldServerEntTracker entTracker,
    WorldEntStreamer entStreamer)
{
    public void Tick()
    {
        entTracker.Tick();
        backend.Frame();
        backend.Tick();
    }

    public void Stream()
    {
        entStreamer.Stream();
    }
}
