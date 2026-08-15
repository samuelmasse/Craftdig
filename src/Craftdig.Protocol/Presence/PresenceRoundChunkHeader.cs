namespace Craftdig;

public readonly record struct PresenceRoundChunkHeader(
    Hash256 RoundHash,
    ushort ChunkIndex,
    ushort ChunkCount,
    uint TotalChallengeCount);
