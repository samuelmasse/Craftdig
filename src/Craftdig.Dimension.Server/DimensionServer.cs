namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionServer(
    AppLog log,
    DimensionContext context,
    DimensionBackend backend,
    DimensionInventoryActions inventoryActions,
    DimensionPendingMovement pendingMovement,
    DimensionEntStreamer entStreamer,
    DimensionPositionStreamer positionStreamer,
    DimensionChunkStreamerRequester chunkStreamerRequester,
    DimensionForgottenChunks forgottenChunks,
    DimensionForgottenSections forgottenSections,
    DimensionSectionUpdateStreamer sectionUpdateStreamer,
    DimensionLightUpdateStreamer lightUpdateStreamer,
    DimensionSectionReminder sectionReminder,
    DimensionRigidBag rigidBag,
    DimensionPlayerSpawner playerSpawner,
    DimensionPlayerSocketsCleaner playerSocketsCleaner)
{
    public void Tick()
    {
        context.Frame();

        playerSpawner.Tick();
        forgottenSections.Tick();
        forgottenChunks.Tick();
        playerSocketsCleaner.Tick();
        inventoryActions.Tick();
        pendingMovement.Tick();

        context.Tick();
        backend.Tick();

        backend.Frame();

        log.Trace("{0} rigids", rigidBag.Ents.Length);
    }

    public void Stream()
    {
        entStreamer.Stream();
        chunkStreamerRequester.Tick();
        sectionUpdateStreamer.Tick();
        lightUpdateStreamer.Tick();
        sectionReminder.Tick();
        positionStreamer.Tick();
    }
}
