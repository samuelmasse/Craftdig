namespace Craftdig.Server;

public sealed class ServerPresenceRound
{
    public const int ProofBatchRecordCapacity = 16;

    private readonly PresenceProofRecord[] pendingProofs = new PresenceProofRecord[ProofBatchRecordCapacity];
    private int pendingProofCount;
    private long pendingProofTimestamp;

    public ServerPresenceRound(long id, long startedTimestamp, PresenceChallengeRecord[] records)
    {
        Id = id;
        StartedTimestamp = startedTimestamp;
        Records = records;

        if (!PresenceRoundDigest.TryCompute(records, out var roundHash))
            throw new InvalidDataException("The presence round was not canonically ordered.");

        RoundHash = roundHash;
        Chunks = EncodeChunks(records, roundHash);
    }

    public readonly long Id;
    public readonly long StartedTimestamp;
    public readonly Hash256 RoundHash;
    public readonly PresenceChallengeRecord[] Records;
    public readonly byte[][] Chunks;
    public readonly List<byte[]> ProofBatches = [];

    public bool Contains(SessionId sessionId) =>
        PresenceChallengeRecords.ContainsSession(Records, sessionId);

    public void AddProof(PresenceProofRecord proof, long timestamp)
    {
        if (pendingProofCount == 0)
            pendingProofTimestamp = timestamp;

        pendingProofs[pendingProofCount++] = proof;
        if (pendingProofCount == pendingProofs.Length)
            FlushProofs();
    }

    public void FlushProofsIfDue(long timestamp, long maximumDelay)
    {
        if (pendingProofCount != 0 && timestamp - pendingProofTimestamp >= maximumDelay)
            FlushProofs();
    }

    public void FlushProofs()
    {
        if (pendingProofCount == 0)
            return;

        int bodySize = PresenceProofBatchCommandCodec.HeaderSize + pendingProofCount * PresenceProofRecord.Size;
        var body = new byte[bodySize];
        if (!PresenceProofBatchCommandCodec.TryWrite(
                body,
                RoundHash,
                pendingProofs.AsSpan(0, pendingProofCount),
                out int written) || written != body.Length)
            throw new InvalidDataException("The presence proof batch could not be encoded.");

        ProofBatches.Add(body);
        pendingProofCount = 0;
    }

    private static byte[][] EncodeChunks(PresenceChallengeRecord[] records, Hash256 roundHash)
    {
        int chunkCount = (records.Length + PresenceRoundChunkCommandCodec.MaxRecordCount - 1) /
            PresenceRoundChunkCommandCodec.MaxRecordCount;
        var chunks = new byte[chunkCount][];

        for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
        {
            int recordOffset = chunkIndex * PresenceRoundChunkCommandCodec.MaxRecordCount;
            int recordCount = Math.Min(
                PresenceRoundChunkCommandCodec.MaxRecordCount,
                records.Length - recordOffset);
            int bodySize = PresenceRoundChunkCommandCodec.HeaderSize + recordCount * PresenceChallengeRecord.Size;
            var body = new byte[bodySize];
            var header = new PresenceRoundChunkHeader(
                roundHash,
                (ushort)chunkIndex,
                (ushort)chunkCount,
                (uint)records.Length);
            if (!PresenceRoundChunkCommandCodec.TryWrite(
                    body,
                    header,
                    records.AsSpan(recordOffset, recordCount),
                    out int written) || written != body.Length)
                throw new InvalidDataException("The presence round chunk could not be encoded.");

            chunks[chunkIndex] = body;
        }

        return chunks;
    }
}
