namespace Crafthoe.Dimension.Server;

[Dimension]
public class DimensionServer(
    DimensionContext context,
    DimensionBackend backend,
    DimensionSocketCleaner socketCleaner,
    DimensionPlayerSpawner playerSpawner,
    DimensionPositionStreamer positionStreamer,
    DimensionChunkStreamerRequester chunkStreamerRequester)
{
    public void Tick()
    {
        context.Frame();
        backend.Frame();
        socketCleaner.Tick();
        chunkStreamerRequester.Tick();
        backend.Tick();
        playerSpawner.Tick();
        positionStreamer.Tick();
    }
}
