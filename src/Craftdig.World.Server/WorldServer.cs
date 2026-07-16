namespace Craftdig.World.Server;

[World]
public class WorldServer(
    WorldBackend backend,
    WorldEntStreamer entStreamer)
{
    public void Tick()
    {
        backend.Frame();
        backend.Tick();
    }

    public void Stream()
    {
        entStreamer.Stream();
    }
}
