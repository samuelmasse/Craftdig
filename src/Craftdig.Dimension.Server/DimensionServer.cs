namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionServer(
    AppLog log,
    DimensionContext context,
    DimensionBackend backend,
    DimensionPendingMovement pendingMovement,
    DimensionPositionStreamer positionStreamer,
    DimensionChunkStreamerRequester chunkStreamerRequester,
    DimensionForgottenChunks forgottenChunks,
    DimensionForgottenSections forgottenSections,
    DimensionSectionUpdateStreamer sectionUpdateStreamer,
    DimensionSectionReminder sectionReminder,
    DimensionRigidBag rigidBag,
    DimensionPlayerSpawner playerSpawner,
    DimensionPlayerSocketsCleaner playerSocketsCleaner)
{
    public void Tick()
    {
        context.Frame();
        backend.Frame();

        playerSpawner.Tick();
        playerSocketsCleaner.Tick();
        forgottenSections.Tick();
        forgottenChunks.Tick();
        pendingMovement.Tick();

        context.Tick();
        backend.Tick();

        chunkStreamerRequester.Tick();
        sectionUpdateStreamer.Tick();
        sectionReminder.Tick();
        positionStreamer.Tick();

        log.Debug("{0} rigids", rigidBag.Ents.Length);
    }
}
