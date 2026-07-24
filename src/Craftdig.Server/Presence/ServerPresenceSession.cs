namespace Craftdig.Server;

public sealed class ServerPresenceSession(ServerPresenceConnection connection, int identityQueueCapacity) : IDisposable
{
    private readonly Queue<ValidatedIdentityTicket> identityQueue = [];
    private readonly int identityQueueCapacity = identityQueueCapacity;
    private ECDsa? publicKey;
    public readonly ServerPresenceConnection Connection = connection;
    public ServerIdentitySessionSnapshot? Identity;
    public Hash256 ServerContextHash;
    public ulong LatestChallengeSequence;
    public PresenceChallenge? LatestChallenge;
    public long LatestChallengeTimestamp;
    public long OutputRoundId;
    public int NextRoundChunk;
    public int NextProofBatch;
    public long DrainingRoundId;
    public int NextDrainingProofBatch;
    public long LastProofRoundId;
    public long PriorProofRoundId;
    public int RejectedProofInputs;
    public int SlowSendStrikes;
    public long NextSendAttemptTimestamp;
    public bool InitialIdentitySnapshotPending = true;
    public bool PreferIdentityOutput = true;

    public bool IsVerified => Identity != null;
    public int QueuedIdentityCount => identityQueue.Count;

    public void InstallIdentity(ServerIdentitySessionSnapshot identity)
    {
        Identity = identity;
        ServerContextHash = identity.Ticket.ServerContext.ComputeHash();
        publicKey ??= identity.Ticket.PublicKey.CreateEcdsa();
    }

    public bool VerifyProof(Hash256 digest, P256Signature signature) =>
        publicKey != null && signature.VerifyHash(publicKey, digest);

    public bool HasProofFor(long roundId) =>
        LastProofRoundId == roundId || PriorProofRoundId == roundId;

    public void RecordProofFor(long roundId)
    {
        PriorProofRoundId = LastProofRoundId;
        LastProofRoundId = roundId;
    }

    public bool TryEnqueueIdentity(ValidatedIdentityTicket ticket)
    {
        if (identityQueue.Count >= identityQueueCapacity)
            return false;

        identityQueue.Enqueue(ticket);
        return true;
    }

    public bool TryPeekIdentity([MaybeNullWhen(false)] out ValidatedIdentityTicket ticket) =>
        identityQueue.TryPeek(out ticket);

    public void DequeueIdentity() => identityQueue.Dequeue();

    public void Dispose() => publicKey?.Dispose();
}
