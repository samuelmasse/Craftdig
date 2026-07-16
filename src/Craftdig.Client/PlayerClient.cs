namespace Craftdig.Client;

[Player]
public class PlayerClient(
    DimensionRemoteInterpolation remoteInterpolation,
    PlayerPositionUpdateReceiver positionUpdateReceiver,
    PlayerPings pings,
    PlayerPosition position,
    PlayerChunks chunks,
    PlayerEntSync entSync,
    PlayerInventoryClient inventory,
    PlayerSections sections,
    PlayerAheadSections aheadSections)
{
    public void Tick()
    {
        position.Tick();
        aheadSections.Tick();
    }

    public void Sync()
    {
        entSync.Frame();
        inventory.Frame();
    }

    public bool Prepare() => position.Prepare();

    public void Stream()
    {
        if (positionUpdateReceiver.Count == 0)
            return;

        position.Stream();
    }

    public void Frame()
    {
        remoteInterpolation.Frame();
        chunks.Frame();
        sections.Frame();
        pings.Frame();
    }
}
