namespace Craftdig.Protocol;

public static class PresenceRoundChunkCommandCodec
{
    public const int HeaderSize = Hash256.Size + sizeof(ushort) + sizeof(ushort) + sizeof(uint);
    public const int MaxRecordCount = ProtocolLimits.MaxPresenceRoundRecordsPerChunk;
    public const int MaxSize = HeaderSize + MaxRecordCount * PresenceChallengeRecord.Size;

    public static bool TryRead(
        ReadOnlySpan<byte> source,
        out PresenceRoundChunkHeader header,
        out ReadOnlySpan<byte> recordBytes)
    {
        header = default;
        recordBytes = default;
        var reader = new SpanReader(source);
        var roundHash = reader.Read<Hash256>();
        ushort chunkIndex = reader.ReadUInt16();
        ushort chunkCount = reader.ReadUInt16();
        uint totalChallengeCount = reader.ReadUInt32();
        var payload = reader.ReadRest();
        if (!reader.Finish() ||
            !WireRecords.HasShape<PresenceChallengeRecord>(payload, 1, MaxRecordCount) ||
            !IsValidHeader(chunkIndex, chunkCount, totalChallengeCount, WireRecords.Count<PresenceChallengeRecord>(payload)) ||
            !PresenceChallengeRecords.AreCanonical(payload))
            return false;

        header = new(roundHash, chunkIndex, chunkCount, totalChallengeCount);
        recordBytes = payload;
        return true;
    }

    public static bool TryWrite(
        Span<byte> destination,
        PresenceRoundChunkHeader header,
        ReadOnlySpan<PresenceChallengeRecord> records,
        out int written)
    {
        written = 0;
        if (records.Length > MaxRecordCount ||
            !IsValidHeader(header.ChunkIndex, header.ChunkCount, header.TotalChallengeCount, records.Length) ||
            !PresenceChallengeRecords.AreCanonical(records))
            return false;

        var writer = new SpanWriter(destination);
        writer.Write(header.RoundHash);
        writer.WriteUInt16(header.ChunkIndex);
        writer.WriteUInt16(header.ChunkCount);
        writer.WriteUInt32(header.TotalChallengeCount);
        WireRecords.WriteAll(ref writer, records);
        return writer.Finish(out written);
    }

    private static bool IsValidHeader(ushort chunkIndex, ushort chunkCount, uint totalChallengeCount, int recordCount) =>
        chunkCount is > 0 and <= ProtocolLimits.MaxPresenceRoundChunks &&
        chunkIndex < chunkCount &&
        totalChallengeCount is > 0 and <= ProtocolLimits.MaxPresencePlayers &&
        recordCount is > 0 and <= MaxRecordCount &&
        recordCount <= totalChallengeCount &&
        chunkCount <= totalChallengeCount;
}
