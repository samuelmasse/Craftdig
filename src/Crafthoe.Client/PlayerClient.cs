namespace Crafthoe.Client;

[Player]
public class PlayerClient(
    PlayerPings pings,
    PlayerPosition position,
    PlayerChunks chunks)
{
    public void Tick()
    {
        position.Tick();
    }

    public void Stream()
    {
        position.Stream();
    }

    public void Frame()
    {
        chunks.Frame();
        pings.Frame();
    }
}
