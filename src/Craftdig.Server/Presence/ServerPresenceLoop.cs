namespace Craftdig;

[Server]
public class ServerPresenceLoop
{
    private static readonly long RoundInterval = MonotonicTime.DurationTicks(TimeSpan.FromSeconds(10));
    private static readonly long ChallengeLifetime = MonotonicTime.DurationTicks(TimeSpan.FromSeconds(15));
    private static readonly long ProofBatchDelay = MonotonicTime.DurationTicks(TimeSpan.FromMilliseconds(100));
    private static readonly long MetricsInterval = MonotonicTime.DurationTicks(TimeSpan.FromMinutes(1));

    private readonly Log log;
    private readonly ServerIdentitySessionEvents inbox;
    private readonly ServerPresenceMetrics metrics;
    private readonly ServerPresenceSessions sessions;
    private readonly ServerPresenceIntake intake;
    private readonly ServerPresenceFanout fanout;
    private readonly double egressRate;

    private Thread? thread;
    private volatile bool stop;
    private long nextRoundId;
    private ServerPresenceRound? currentRound;
    private ServerPresenceRound? previousRound;

    public ServerPresenceLoop(
        Log log,
        ServerConfig config,
        ServerIdentitySessionEvents inbox,
        ServerPresenceMetrics metrics)
    {
        this.log = log;
        this.inbox = inbox;
        this.metrics = metrics;
        egressRate = Math.Max(
            PresenceRoundChunkCommandCodec.MaxSize + ProtocolLimits.FrameHeaderSize,
            config.PresenceEgressBytesPerSecond);
        sessions = new(log, metrics, config.MaxPlayers, config.MaxPlayers + 16);
        intake = new(inbox, metrics, sessions);
        fanout = new(metrics, sessions, egressRate);
    }

    public void Start()
    {
        if (thread != null)
            return;

        stop = false;
        thread = new Thread(Run)
        {
            IsBackground = true,
            Name = "Craftdig presence",
        };
        thread.Start();
    }

    public void Stop()
    {
        stop = true;
        inbox.Signal();
    }

    public void Join()
    {
        thread?.Join();
        log.Info("Presence loop stopped");
    }

    private void Run()
    {
        long now = Stopwatch.GetTimestamp();
        long nextRoundTimestamp = now + RoundInterval;
        long nextMetricsTimestamp = now + MetricsInterval;
        intake.Initialize(now);
        fanout.Initialize(now);
        log.Info("Presence loop started with a {0:N0} byte/s egress budget", egressRate);

        try
        {
            while (!stop)
            {
                now = Stopwatch.GetTimestamp();
                sessions.RemoveDisconnected(inbox);
                sessions.DrainRegistrations(inbox);
                sessions.ApplyActivated();
                intake.DrainChallenges();
                intake.DrainProofs(now, currentRound, previousRound);

                if (now >= nextRoundTimestamp)
                {
                    FreezeRound(now);
                    nextRoundTimestamp = now + RoundInterval;
                }

                currentRound?.FlushProofsIfDue(now, ProofBatchDelay);
                previousRound?.FlushProofsIfDue(now, ProofBatchDelay);
                fanout.Refill(now);
                fanout.Schedule(now, currentRound, previousRound);
                PublishMetrics(now, ref nextMetricsTimestamp);
                inbox.Wait(TimeSpan.FromMilliseconds(2));
            }
        }
        catch (Exception e)
        {
            log.Error("Presence loop crashed", e);
        }
        finally
        {
            sessions.Shutdown(inbox);
        }
    }

    private void FreezeRound(long now)
    {
        currentRound?.FlushProofs();
        previousRound = currentRound;
        RotateSessionsToDraining();

        var records = CollectChallengeRecords(now);
        metrics.RecordRound(records.Count);
        if (records.Count == 0)
        {
            currentRound = null;
            return;
        }

        currentRound = new(++nextRoundId, now, [.. records]);
        foreach (var session in sessions.All)
        {
            session.OutputRoundId = currentRound.Id;
            session.NextRoundChunk = 0;
            session.NextProofBatch = 0;
        }
    }

    private void RotateSessionsToDraining()
    {
        foreach (var session in sessions.All)
        {
            if (previousRound != null && session.OutputRoundId == previousRound.Id)
            {
                session.DrainingRoundId = previousRound.Id;
                session.NextDrainingProofBatch = session.NextProofBatch;
            }
            else
            {
                session.DrainingRoundId = 0;
                session.NextDrainingProofBatch = 0;
            }

            session.OutputRoundId = 0;
            session.RejectedProofInputs = 0;
        }
    }

    private List<PresenceChallengeRecord> CollectChallengeRecords(long now)
    {
        var records = new List<PresenceChallengeRecord>(sessions.All.Count);
        foreach (var session in sessions.All)
        {
            if (session.Connection.IsCanceled ||
                session.LatestChallenge is not { } challenge ||
                now - session.LatestChallengeTimestamp > ChallengeLifetime)
                continue;

            records.Add(new(session.Connection.SessionId, challenge.Sequence, challenge.Nonce));
        }

        records.Sort();
        return records;
    }

    private void PublishMetrics(long now, ref long nextMetricsTimestamp)
    {
        metrics.SetQueueDepths(inbox.Depths());
        metrics.SetCurrentRoundAge(currentRound == null ? 0 :
            (long)((now - currentRound.StartedTimestamp) * 1000d / Stopwatch.Frequency));
        if (now < nextMetricsTimestamp)
            return;

        nextMetricsTimestamp = now + MetricsInterval;
        var value = metrics.Snapshot();
        log.Info($"Presence metrics: active={value.ActiveSessions}, " +
            $"inbox={value.LifecycleQueueDepth}/{value.ChallengeQueueDepth}/{value.ProofQueueDepth}, " +
            $"rounds={value.Rounds}, roundAgeMs={value.CurrentRoundAgeMilliseconds}, " +
            $"lastChallenges={value.LastRoundChallenges}, " +
            $"challenges={value.ChallengesAccepted}/{value.ChallengesRejected}, " +
            $"proofs={value.ProofsAccepted}/{value.ProofsRejected}, bytes={value.BytesQueued}, " +
            $"backpressure={value.BackpressureEvents}, slowDisconnects={value.SlowSocketDisconnects}");
    }
}
