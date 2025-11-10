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
            var ping = new PingCommand() { Timestamp = Stopwatch.GetTimestamp() };

            socket.Send(new((int)CommonCommand.Ping,
                MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref ping, 1))));

            lastPing = now;
        }
    }
}
