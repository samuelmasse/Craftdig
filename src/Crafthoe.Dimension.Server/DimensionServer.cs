namespace Crafthoe.Server;

[Dimension]
public class DimensionServer(
    DimensionContext context,
    DimensionServerContext serverContext,
    DimensionSocketCleaner socketCleaner,
    DimensionPlayerSpawner playerSpawner,
    DimensionPositionStreamer positionStreamer,
    DimensionChunkStreamerRequester chunkStreamerRequester)
{
    public void Tick()
    {
        context.Frame();
        serverContext.Frame();
        socketCleaner.Tick();
        chunkStreamerRequester.Tick();
        serverContext.Tick();
        playerSpawner.Tick();
        positionStreamer.Tick();
    }
}
