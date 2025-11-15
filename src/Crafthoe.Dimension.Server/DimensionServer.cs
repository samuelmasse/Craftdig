namespace Crafthoe.Dimension.Server;

[Dimension]
public class DimensionServer(
    DimensionContext context,
    DimensionBackend backend,
    DimensionPendingMovement pendingMovement,
    DimensionSocketCleaner socketCleaner,
    DimensionPlayerSpawner playerSpawner,
    DimensionPositionStreamer positionStreamer,
    DimensionChunkStreamerRequester chunkStreamerRequester,
    DimensionForgottenChunks forgottenChunks)
{
    public void Tick()
    {
        backend.Frame();
        context.Frame();
        socketCleaner.Tick();
        chunkStreamerRequester.Tick();
        forgottenChunks.Tick();
        pendingMovement.Tick();
        context.Tick();
        playerSpawner.Tick();
        positionStreamer.Tick();
    }
}
