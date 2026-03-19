namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionServer(
    AppLog log,
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
    DimensionSectionReminder sectionReminder,
    DimensionRigidBag rigidBag)
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
        backend.Tick();
        playerSpawner.Tick();
        positionStreamer.Tick();
        log.Debug("{0} rigids", rigidBag.Ents.Length);
    }
}
