namespace Craftdig.Protocol;

public static class PresenceRoundDigest
{
    private static ReadOnlySpan<byte> DomainSeparator => "Craftdig presence round v1\0"u8;

    public static bool TryCompute(ReadOnlySpan<PresenceChallengeRecord> records, out Hash256 roundHash)
    {
        roundHash = default;
        if (records.Length is < 1 or > ProtocolLimits.MaxPresencePlayers || !PresenceChallengeRecords.AreCanonical(records))
            return false;

        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(DomainSeparator);

        Span<byte> count = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32BigEndian(count, (uint)records.Length);
        hash.AppendData(count);

        Span<byte> recordBytes = stackalloc byte[PresenceChallengeRecord.Size];
        foreach (ref readonly var record in records)
        {
            record.TryWrite(recordBytes);
            hash.AppendData(recordBytes);
        }

        Span<byte> digest = stackalloc byte[Hash256.Size];
        if (!hash.TryGetHashAndReset(digest, out int written) || written != Hash256.Size)
            throw new CryptographicException("SHA-256 did not produce a 32-byte presence round digest.");

        return Hash256.TryRead(digest, out roundHash);
    }
}
