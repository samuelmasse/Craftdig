namespace Craftdig.Server;

[Server]
public class ServerTick(
    WorldBackend backend,
    WorldDimensionBag dimensions,
    ServerKicker kicker,
    ServerEntTracker entTracker,
    ServerEntStreamer entStreamer,
    ServerPlayerSpawner playerSpawner,
    ServerPlayerSocketsCleaner playerSocketsCleaner)
{
    public void Tick()
    {
        entTracker.Tick();
        playerSpawner.Tick();
        playerSocketsCleaner.Tick();
        kicker.Tick();
        backend.Frame();
        backend.Tick();

        foreach (var dimension in dimensions.Ents)
            dimension.DimensionScope.Get<DimensionServer>().Tick();

        entStreamer.Stream();
    }
}
