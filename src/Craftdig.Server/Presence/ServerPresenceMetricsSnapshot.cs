namespace Craftdig.Server;

public readonly record struct ServerPresenceMetricsSnapshot(
    long ActiveSessions,
    long LifecycleQueueDepth,
    long ChallengeQueueDepth,
    long ProofQueueDepth,
    long Rounds,
    long CurrentRoundAgeMilliseconds,
    long LastRoundChallenges,
    long ChallengesAccepted,
    long ChallengesRejected,
    long ProofsAccepted,
    long ProofsRejected,
    long BytesQueued,
    long BackpressureEvents,
    long SlowSocketDisconnects);
