namespace Craftdig.Protocol;

public readonly record struct PresenceRoundChunkHeader(
    Hash256 RoundHash,
    ushort ChunkIndex,
    ushort ChunkCount,
    uint TotalChallengeCount);
