namespace Craftdig.Client;

internal sealed class PlayerChallengeScheduler(
    Log log,
    PlayerIdentitySession identitySession,
    PlayerPresenceInbox inbox,
    PlayerSocket socket)
{
    private static readonly TimeSpan ChallengeInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan ChallengeFreshness = TimeSpan.FromSeconds(15);

    private readonly Lock gate = new();
    private readonly List<(ulong Sequence, Nonce256 Nonce, long Deadline)> pendingChallenges = [];

    public (ulong Sequence, Nonce256 Nonce, long Deadline)[] Snapshot()
    {
        lock (gate)
            return [.. pendingChallenges];
    }

    public void Clear()
    {
        lock (gate)
            pendingChallenges.Clear();
    }

    public void Run(CancellationToken cancellationToken)
    {
        try
        {
            RunSchedule(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception e)
        {
            log.Warn("Presence challenge worker failed with {0}; disconnecting", e.GetType().Name);
            socket.Disconnect();
        }
    }

    private void RunSchedule(CancellationToken cancellationToken)
    {
        long intervalTicks = MonotonicTime.DurationTicks(ChallengeInterval);
        long offsetTicks = intervalTicks * OffsetMilliseconds() / 5000;
        long now = Stopwatch.GetTimestamp();
        long next = now - now % intervalTicks + offsetTicks;
        if (next <= now)
            next += intervalTicks;

        ulong sequence = 0;
        Span<byte> body = stackalloc byte[PresenceChallengeCommandCodec.Size];
        while (!cancellationToken.IsCancellationRequested && socket.Connected)
        {
            now = Stopwatch.GetTimestamp();
            if (next > now && cancellationToken.WaitHandle.WaitOne(MonotonicTime.ToTimeSpan(next - now)))
                return;

            if (sequence == ulong.MaxValue)
            {
                log.Warn("Presence challenge sequence exhausted; disconnecting");
                socket.Disconnect();
                return;
            }

            var challenge = new PresenceChallenge(++sequence, Nonce256.CreateRandom());
            long deadline = Stopwatch.GetTimestamp() + MonotonicTime.DurationTicks(ChallengeFreshness);
            lock (gate)
            {
                pendingChallenges.Add((challenge.Sequence, challenge.Nonce, deadline));
                if (pendingChallenges.Count > 2)
                    pendingChallenges.RemoveAt(0);
            }

            PresenceChallengeCommandCodec.TryWrite(body, challenge, out int written);
            socket.SendRaw<PresenceChallengeCommand>(body[..written]);
            inbox.SignalWork();

            next += intervalTicks;
            now = Stopwatch.GetTimestamp();
            if (next <= now)
                next = now + intervalTicks;
        }
    }

    private int OffsetMilliseconds()
    {
        if (!identitySession.IdentityEnabled)
            return RandomNumberGenerator.GetInt32(5000);

        ReadOnlySpan<byte> label = "challenge-offset"u8;
        Span<byte> input = stackalloc byte[SessionId.Size + label.Length];
        identitySession.SessionId.TryWrite(input);
        label.CopyTo(input[SessionId.Size..]);
        Span<byte> digest = stackalloc byte[Hash256.Size];
        Hash256.Compute(input).TryWrite(digest);
        return (int)(BinaryPrimitives.ReadUInt32BigEndian(digest) % 5000);
    }
}
