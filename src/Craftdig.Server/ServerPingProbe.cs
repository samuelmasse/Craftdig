namespace Craftdig;

public class ServerPingProbe
{
    private long pending;
    private long received;
    private ushort milliseconds;
    private bool rosterOpen;

    internal ushort? Read(long now)
    {
        lock (this)
            return received != 0 && Stopwatch.GetElapsedTime(received, now) < RosterPing.MaxAge ? milliseconds : null;
    }

    internal void Send(NetSocket socket, ReadOnlySpan<RosterPing> samples)
    {
        lock (this)
        {
            long now = Stopwatch.GetTimestamp();

            if (pending != 0 && Stopwatch.GetElapsedTime(pending, now) < RosterPing.MaxAge)
                return;

            var ping = new PingCommand { Timestamp = now };
            var header = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref ping, 1));
            var payload = rosterOpen ? MemoryMarshal.AsBytes(samples) : [];
            pending = socket.TrySend(PingCommand.CommandId, header, payload) ? now : 0;
        }
    }

    internal void Receive(PongCommand pong)
    {
        lock (this)
        {
            if (pending == 0 || pong.Ping.Timestamp != pending)
                return;

            long now = Stopwatch.GetTimestamp();
            var elapsed = Stopwatch.GetElapsedTime(pending, now);
            pending = 0;

            if (elapsed >= RosterPing.MaxAge)
                return;

            received = now;
            milliseconds = (ushort)elapsed.TotalMilliseconds;
            rosterOpen = pong.RosterOpen;
        }
    }
}
