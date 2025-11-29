namespace Crafthoe.Client;

[Player]
public class PlayerClient(
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
        position.Stream();
    }

    public void Frame()
    {
        chunks.Frame();
        sections.Frame();
        pings.Frame();
    }
}
