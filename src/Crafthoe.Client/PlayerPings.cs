namespace Crafthoe.Client;

[Player]
public class PlayerPings(PlayerSocket socket)
{
    private DateTime lastPing;

    public void Frame()
    {
        var now = DateTime.UtcNow;
        var delta = now - lastPing;

        if (delta.TotalMilliseconds > 500)
        {
            socket.Send(new PingCommand() { Timestamp = Stopwatch.GetTimestamp() });

            lastPing = now;
        }
    }
}
