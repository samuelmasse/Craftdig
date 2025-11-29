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
    DimensionForgottenChunks forgottenChunks,
    DimensionForgottenSections forgottenSections,
    DimensionSectionUpdateStreamer sectionUpdateStreamer,
    DimensionSectionReminder sectionReminder)
{
    public void Tick()
    {
        backend.Frame();
        forgottenSections.Tick();
        sectionUpdateStreamer.Tick();
        sectionReminder.Tick();
        forgottenChunks.Tick();
        chunkStreamerRequester.Tick();
        context.Frame();
        socketCleaner.Tick();
        pendingMovement.Tick();
        context.Tick();
        playerSpawner.Tick();
        positionStreamer.Tick();
    }
}
