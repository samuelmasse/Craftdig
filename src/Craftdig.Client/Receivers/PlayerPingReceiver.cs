namespace Craftdig;

[Player]
public class PlayerPingReceiver(PlayerIdentityCache identityCache)
{
    private readonly Dictionary<Guid, ushort> samples = [];
    private long received;

    public ushort? Milliseconds(Guid playerId)
    {
        lock (samples)
        {
            if (Stopwatch.GetElapsedTime(received) < RosterPing.MaxAge && samples.TryGetValue(playerId, out var value))
                return value;

            return null;
        }
    }

    public void Receive(NetSocket source, ReadOnlySpan<byte> body)
    {
        int headerSize = Unsafe.SizeOf<PingCommand>();
        int recordSize = Unsafe.SizeOf<RosterPing>();
        int bytes = body.Length - headerSize;

        if (bytes < 0 || bytes % recordSize != 0 || bytes / recordSize > ProtocolLimits.MaxPresencePlayers)
        {
            source.Disconnect();
            return;
        }

        source.Send(new PongCommand { Ping = MemoryMarshal.Read<PingCommand>(body), RosterOpen = identityCache.IsPlayerListOpen });
        var records = MemoryMarshal.Cast<byte, RosterPing>(body[headerSize..]);

        lock (samples)
        {
            samples.Clear();

            foreach (var record in records)
                samples[record.PlayerId] = record.Milliseconds;

            received = Stopwatch.GetTimestamp();
        }
    }
}
