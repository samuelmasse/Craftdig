namespace Craftdig.Client;

public class PlayerPresenceRound(PresenceRoundChunkHeader firstHeader, long order)
{
    private readonly byte[]?[] chunks = new byte[firstHeader.ChunkCount][];
    private int receivedChunks;
    private int receivedRecords;

    public readonly Hash256 RoundHash = firstHeader.RoundHash;
    public readonly ushort ChunkCount = firstHeader.ChunkCount;
    public readonly uint TotalChallengeCount = firstHeader.TotalChallengeCount;
    public readonly long Order = order;
    public bool Invalid;
    public bool Complete;
    public bool Usable;
    public bool Signed;
    public PlayerPresenceRoundFault Fault;
    public long LocalDeadline;
    public SessionId OwnRoundSessionId;
    public PresenceChallengeRecord[] Records = [];

    public void AddChunk(PresenceRoundChunkHeader header, ReadOnlySpan<byte> recordBytes)
    {
        if (Invalid)
            return;

        int recordCount = WireRecords.Count<PresenceChallengeRecord>(recordBytes);
        if (Complete ||
            header.RoundHash != RoundHash ||
            header.ChunkCount != ChunkCount ||
            header.TotalChallengeCount != TotalChallengeCount ||
            chunks[header.ChunkIndex] != null ||
            receivedRecords + recordCount > TotalChallengeCount)
        {
            Reject(PlayerPresenceRoundFault.ChunkMismatch);
            return;
        }

        chunks[header.ChunkIndex] = recordBytes.ToArray();
        receivedChunks++;
        receivedRecords += recordCount;
    }

    public void CompleteRound(
        ReadOnlySpan<(ulong Sequence, Nonce256 Nonce, long Deadline)> pendingChallenges,
        long now)
    {
        if (Invalid || Complete || receivedChunks != ChunkCount)
            return;

        Complete = true;
        if (receivedRecords != TotalChallengeCount || !TryAssembleRecords(out var records))
        {
            Reject(PlayerPresenceRoundFault.ChunkMismatch);
            return;
        }

        if (!PresenceRoundDigest.TryCompute(records, out var computedHash) || computedHash != RoundHash)
        {
            Reject(PlayerPresenceRoundFault.DigestMismatch);
            return;
        }

        Records = records;
        MatchOwnChallenge(pendingChallenges, now);
    }

    public bool ContainsSession(SessionId sessionId) =>
        PresenceChallengeRecords.ContainsSession(Records, sessionId);

    private bool TryAssembleRecords([NotNullWhen(true)] out PresenceChallengeRecord[]? records)
    {
        records = new PresenceChallengeRecord[receivedRecords];
        int recordIndex = 0;
        foreach (var chunk in chunks)
        {
            if (chunk == null)
            {
                records = null;
                return false;
            }

            for (int offset = 0; offset < chunk.Length; offset += PresenceChallengeRecord.Size)
            {
                if (!PresenceChallengeRecord.TryRead(
                        chunk.AsSpan(offset, PresenceChallengeRecord.Size),
                        out records[recordIndex++]))
                {
                    records = null;
                    return false;
                }
            }
        }

        return true;
    }

    private void MatchOwnChallenge(
        ReadOnlySpan<(ulong Sequence, Nonce256 Nonce, long Deadline)> pendingChallenges,
        long now)
    {
        int ownMatches = 0;
        long ownDeadline = 0;
        SessionId ownSessionId = default;
        foreach (ref readonly var record in Records.AsSpan())
        {
            foreach (ref readonly var challenge in pendingChallenges)
            {
                if (record.Sequence == challenge.Sequence && record.Nonce == challenge.Nonce)
                {
                    ownMatches++;
                    ownDeadline = challenge.Deadline;
                    ownSessionId = record.SessionId;
                }
            }
        }

        if (ownMatches == 1 && now <= ownDeadline)
        {
            Usable = true;
            LocalDeadline = ownDeadline;
            OwnRoundSessionId = ownSessionId;
        }
        else
        {
            Fault = PlayerPresenceRoundFault.OwnChallengeUnusable;
        }
    }

    private void Reject(PlayerPresenceRoundFault fault)
    {
        Invalid = true;
        Usable = false;
        Fault = fault;
    }
}
