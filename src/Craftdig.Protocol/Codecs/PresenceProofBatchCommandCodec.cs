namespace Craftdig.Protocol;

public static class PresenceProofBatchCommandCodec
{
    public const int HeaderSize = Hash256.Size;
    public const int MaxRecordCount = ProtocolLimits.MaxPresenceProofRecordsPerBatch;
    public const int MaxSize = HeaderSize + MaxRecordCount * PresenceProofRecord.Size;

    public static bool TryRead(
        ReadOnlySpan<byte> source,
        out Hash256 roundHash,
        out ReadOnlySpan<byte> recordBytes)
    {
        var reader = new SpanReader(source);
        roundHash = reader.Read<Hash256>();
        var payload = reader.ReadRest();
        if (reader.Finish() && WireRecords.HasShape<PresenceProofRecord>(payload, 0, MaxRecordCount))
        {
            recordBytes = payload;
            return true;
        }

        roundHash = default;
        recordBytes = default;
        return false;
    }

    public static bool TryWrite(
        Span<byte> destination,
        Hash256 roundHash,
        ReadOnlySpan<PresenceProofRecord> records,
        out int written)
    {
        written = 0;
        if (records.Length > MaxRecordCount)
            return false;

        var writer = new SpanWriter(destination);
        writer.Write(roundHash);
        WireRecords.WriteAll(ref writer, records);
        return writer.Finish(out written);
    }
}
