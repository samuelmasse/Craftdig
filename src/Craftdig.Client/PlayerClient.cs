namespace Craftdig.Client;

[Player]
public class PlayerClient(
    PlayerPositionUpdateReceiver positionUpdateReceiver,
    PlayerPings pings,
    PlayerPosition position,
    PlayerChunks chunks,
    PlayerSections sections,
    PlayerAheadSections aheadSections)
{
    public void Tick()
    {
        position.Tick();
        aheadSections.Tick();
    }

    public void Stream()
    {
        if (positionUpdateReceiver.Count == 0)
            return;

        position.Stream();
    }

    public void Frame()
    {
        chunks.Frame();
        sections.Frame();
        pings.Frame();
    }
}
