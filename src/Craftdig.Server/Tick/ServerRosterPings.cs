namespace Craftdig;

[Server]
public class ServerRosterPings(WorldPlayerSockets players, ServerConfig config)
{
    private readonly RosterPing[] samples = new RosterPing[config.MaxPlayers];
    private long nextProbe;

    public void Tick()
    {
        long now = Stopwatch.GetTimestamp();

        if (now < nextProbe)
            return;

        nextProbe = now + Stopwatch.Frequency;
        int count = 0;

        foreach (var socket in players.Span)
        {
            if (socket.Connected && socket.PingProbe.Read(now) is { } milliseconds)
                samples[count++] = new(socket.SocketWorldPlayer.Id, milliseconds);
        }

        foreach (var socket in players.Span)
        {
            if (socket.Connected)
                socket.PingProbe.Send(socket, samples.AsSpan(0, count));
        }
    }
}
