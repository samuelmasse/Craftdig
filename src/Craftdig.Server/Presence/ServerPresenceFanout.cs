namespace Craftdig;

internal sealed class ServerPresenceFanout(
    ServerPresenceMetrics metrics,
    ServerPresenceSessions sessions,
    double egressRate)
{
    private const int SlowSendStrikeLimit = 3;
    private static readonly long SendRetryDelay = MonotonicTime.DurationTicks(TimeSpan.FromMilliseconds(250));

    private int schedulerIndex;
    private double tokens;
    private double tokenCapacity;
    private long lastTokenTimestamp;

    public void Initialize(long now)
    {
        lastTokenTimestamp = now;
        tokenCapacity = Math.Max(
            PresenceRoundChunkCommandCodec.MaxSize + ProtocolLimits.FrameHeaderSize,
            Math.Min(egressRate / 100d, 256 * 1024d));
        tokens = tokenCapacity;
    }

    public void Refill(long now)
    {
        long elapsed = now - lastTokenTimestamp;
        if (elapsed <= 0)
            return;

        tokens = Math.Min(tokenCapacity, tokens + elapsed * egressRate / Stopwatch.Frequency);
        lastTokenTimestamp = now;
    }

    public void Schedule(long now, ServerPresenceRound? currentRound, ServerPresenceRound? previousRound)
    {
        var all = sessions.All;
        if (all.Count == 0)
            return;

        int idleAttempts = 0;
        while (all.Count != 0 && idleAttempts < all.Count)
        {
            if (schedulerIndex >= all.Count)
                schedulerIndex = 0;

            var session = all[schedulerIndex++];
            var result = TrySendNext(session, now, currentRound, previousRound);
            if (result == SendAttempt.Sent)
                idleAttempts = 0;
            else
                idleAttempts++;
        }
    }

    private SendAttempt TrySendNext(
        ServerPresenceSession session,
        long now,
        ServerPresenceRound? currentRound,
        ServerPresenceRound? previousRound)
    {
        if (session.Connection.IsCancelled || !session.Connection.Socket.Connected ||
            now < session.NextSendAttemptTimestamp)
            return SendAttempt.None;

        var outputRound = SessionOutputRound(session, now, currentRound);
        if (outputRound != null && session.NextRoundChunk < outputRound.Chunks.Length)
            return SendRoundChunk(session, outputRound, now);

        if (session.InitialIdentitySnapshotPending && TrySendInitialSnapshot(session, now, out var snapshotResult))
            return snapshotResult;

        var proofRound = SelectProofRound(session, now, outputRound, previousRound, out bool drainingProof);
        bool hasIdentity = session.TryPeekIdentity(out var ticket);
        if (!hasIdentity && proofRound == null)
            return SendAttempt.None;

        if (hasIdentity && (proofRound == null || session.PreferIdentityOutput))
            return SendIdentity(session, ticket!, now);

        return SendProofBatch(session, proofRound!, drainingProof, now);
    }

    private static ServerPresenceRound? SessionOutputRound(
        ServerPresenceSession session,
        long now,
        ServerPresenceRound? currentRound)
    {
        if (currentRound is not { } round || session.OutputRoundId != round.Id)
            return null;

        if (session.NextRoundChunk < round.Chunks.Length &&
            now - round.StartedTimestamp > ServerPresenceTiming.RoundFanoutWindow)
        {
            session.OutputRoundId = 0;
            return null;
        }

        return round;
    }

    private SendAttempt SendRoundChunk(ServerPresenceSession session, ServerPresenceRound round, long now)
    {
        var result = TryQueue<PresenceRoundChunkCommand>(session, round.Chunks[session.NextRoundChunk], now);
        if (result == SendAttempt.Sent)
            session.NextRoundChunk++;
        return result;
    }

    private bool TrySendInitialSnapshot(ServerPresenceSession session, long now, out SendAttempt result)
    {
        if (!session.TryPeekIdentity(out var ticket))
        {
            session.InitialIdentitySnapshotPending = false;
            result = SendAttempt.None;
            return false;
        }

        result = TryQueue<PlayerIdentityCommand>(session, ticket.RawTicket, now);
        if (result == SendAttempt.Sent)
        {
            session.DequeueIdentity();
            if (session.QueuedIdentityCount == 0)
                session.InitialIdentitySnapshotPending = false;
        }

        return true;
    }

    private static ServerPresenceRound? SelectProofRound(
        ServerPresenceSession session,
        long now,
        ServerPresenceRound? outputRound,
        ServerPresenceRound? previousRound,
        out bool draining)
    {
        draining = false;
        if (previousRound is { } drainingRound && session.DrainingRoundId == drainingRound.Id &&
            now - drainingRound.StartedTimestamp <= ServerPresenceTiming.DrainingRoundLifetime &&
            session.NextDrainingProofBatch < drainingRound.ProofBatches.Count)
        {
            draining = true;
            return drainingRound;
        }

        if (outputRound != null && now - outputRound.StartedTimestamp >= ServerPresenceTiming.ProofFanoutDelay &&
            session.NextProofBatch < outputRound.ProofBatches.Count)
            return outputRound;

        return null;
    }

    private SendAttempt SendIdentity(ServerPresenceSession session, ValidatedIdentityTicket ticket, long now)
    {
        var result = TryQueue<PlayerIdentityCommand>(session, ticket.RawTicket, now);
        if (result == SendAttempt.Sent)
        {
            session.DequeueIdentity();
            session.PreferIdentityOutput = false;
        }
        return result;
    }

    private SendAttempt SendProofBatch(ServerPresenceSession session, ServerPresenceRound round, bool draining, long now)
    {
        int batch = draining ? session.NextDrainingProofBatch : session.NextProofBatch;
        var result = TryQueue<PresenceProofBatchCommand>(session, round.ProofBatches[batch], now);
        if (result == SendAttempt.Sent)
        {
            if (draining)
                session.NextDrainingProofBatch++;
            else
                session.NextProofBatch++;
            session.PreferIdentityOutput = true;
        }
        return result;
    }

    private SendAttempt TryQueue<C>(ServerPresenceSession session, ReadOnlySpan<byte> body, long now)
        where C : ICommand
    {
        int framedSize = ProtocolLimits.FrameHeaderSize + body.Length;
        if (tokens < framedSize)
            return SendAttempt.None;

        if (session.Connection.Socket.TrySendRaw<C>(body))
        {
            tokens -= framedSize;
            session.SlowSendStrikes = 0;
            session.NextSendAttemptTimestamp = 0;
            metrics.RecordBytesQueued(framedSize);
            return SendAttempt.Sent;
        }

        metrics.RecordBackpressure();
        session.SlowSendStrikes++;
        session.NextSendAttemptTimestamp = now + SendRetryDelay;
        if (session.SlowSendStrikes >= SlowSendStrikeLimit)
        {
            metrics.RecordSlowSocketDisconnect();
            sessions.Reject(session.Connection, "remained backpressured by the presence scheduler");
        }

        return SendAttempt.Backpressured;
    }

    private enum SendAttempt
    {
        None,
        Sent,
        Backpressured,
    }
}
