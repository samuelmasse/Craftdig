namespace Craftdig.Server;

[Server]
public class ServerPresenceMetrics
{
    private long activeSessions;
    private long lifecycleQueueDepth;
    private long challengeQueueDepth;
    private long proofQueueDepth;
    private long rounds;
    private long currentRoundAgeMilliseconds;
    private long lastRoundChallenges;
    private long challengesAccepted;
    private long challengesRejected;
    private long proofsAccepted;
    private long proofsRejected;
    private long bytesQueued;
    private long backpressureEvents;
    private long slowSocketDisconnects;

    public ServerPresenceMetricsSnapshot Snapshot() => new(
        Interlocked.Read(ref activeSessions),
        Interlocked.Read(ref lifecycleQueueDepth),
        Interlocked.Read(ref challengeQueueDepth),
        Interlocked.Read(ref proofQueueDepth),
        Interlocked.Read(ref rounds),
        Interlocked.Read(ref currentRoundAgeMilliseconds),
        Interlocked.Read(ref lastRoundChallenges),
        Interlocked.Read(ref challengesAccepted),
        Interlocked.Read(ref challengesRejected),
        Interlocked.Read(ref proofsAccepted),
        Interlocked.Read(ref proofsRejected),
        Interlocked.Read(ref bytesQueued),
        Interlocked.Read(ref backpressureEvents),
        Interlocked.Read(ref slowSocketDisconnects));

    public void SetActiveSessions(int count) => Interlocked.Exchange(ref activeSessions, count);
    public void SetQueueDepths(ServerPresenceInboxDepths depths)
    {
        Interlocked.Exchange(ref lifecycleQueueDepth, depths.Lifecycle);
        Interlocked.Exchange(ref challengeQueueDepth, depths.Challenges);
        Interlocked.Exchange(ref proofQueueDepth, depths.Proofs);
    }

    public void SetCurrentRoundAge(long milliseconds) =>
        Interlocked.Exchange(ref currentRoundAgeMilliseconds, milliseconds);

    public void RecordRound(int challengeCount)
    {
        Interlocked.Increment(ref rounds);
        Interlocked.Exchange(ref lastRoundChallenges, challengeCount);
    }

    public void RecordChallengeAccepted() => Interlocked.Increment(ref challengesAccepted);
    public void RecordChallengeRejected() => Interlocked.Increment(ref challengesRejected);
    public void RecordProofAccepted() => Interlocked.Increment(ref proofsAccepted);
    public void RecordProofRejected() => Interlocked.Increment(ref proofsRejected);
    public void RecordBytesQueued(int count) => Interlocked.Add(ref bytesQueued, count);
    public void RecordBackpressure() => Interlocked.Increment(ref backpressureEvents);
    public void RecordSlowSocketDisconnect() => Interlocked.Increment(ref slowSocketDisconnects);
}
