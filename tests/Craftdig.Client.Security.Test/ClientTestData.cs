namespace Craftdig.Client.Security.Test;

internal static class ClientTestData
{
    public static PresenceChallengeRecord[] Challenges(int count)
    {
        var records = new PresenceChallengeRecord[count];
        Span<byte> sessionBytes = stackalloc byte[SessionId.Size];
        Span<byte> nonceBytes = stackalloc byte[Nonce256.Size];
        for (int i = 0; i < count; i++)
        {
            sessionBytes.Clear();
            sessionBytes[6] = 0x40;
            sessionBytes[8] = 0x80;
            BinaryPrimitives.WriteUInt32BigEndian(sessionBytes[12..], (uint)i);
            Assert.IsTrue(SessionId.TryRead(sessionBytes, out var sessionId));

            nonceBytes.Clear();
            BinaryPrimitives.WriteUInt32BigEndian(nonceBytes[28..], (uint)(i + 1));
            Assert.IsTrue(Nonce256.TryRead(nonceBytes, out var nonce));
            records[i] = new(sessionId, (ulong)(i + 1), nonce);
        }

        return records;
    }

    public static Hash256 Hash(int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        return Hash256.Compute(bytes);
    }

    public static byte[] Encode(ReadOnlySpan<PresenceChallengeRecord> records)
    {
        var encoded = new byte[records.Length * PresenceChallengeRecord.Size];
        for (int i = 0; i < records.Length; i++)
            Assert.IsTrue(records[i].TryWrite(encoded.AsSpan(i * PresenceChallengeRecord.Size)));
        return encoded;
    }
}
