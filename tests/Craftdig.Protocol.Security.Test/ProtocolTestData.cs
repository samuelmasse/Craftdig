namespace Craftdig.Protocol.Security.Test;

internal static class ProtocolTestData
{
    public const string CompactTicket =
        "eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiI2ZTM1NWQxNC01ZDg5LTQ3ZGQtOWIxMi05NjJlNjY3OWU3NTAifQ.c2ln";

    public static byte[] Bytes(string hex) => Convert.FromHexString(hex);

    public static Hash256 Hash(string hex)
    {
        if (!Hash256.TryRead(Bytes(hex), out var value))
            throw new InvalidOperationException("The test hash must contain exactly 32 bytes.");

        return value;
    }

    public static Nonce256 Nonce(string hex)
    {
        if (!Nonce256.TryRead(Bytes(hex), out var value))
            throw new InvalidOperationException("The test nonce must contain exactly 32 bytes.");

        return value;
    }

    public static SessionId Session(string text)
    {
        if (!SessionId.TryFromGuid(Guid.Parse(text), out var value))
            throw new InvalidOperationException("The test session must be a version-4 UUID.");

        return value;
    }

    public static PresenceChallengeRecord[] Challenges(int count)
    {
        var records = new PresenceChallengeRecord[count];
        Span<byte> sessionBytes = stackalloc byte[SessionId.Size];
        Span<byte> nonceBytes = stackalloc byte[Nonce256.Size];

        for (int i = 0; i < records.Length; i++)
        {
            sessionBytes.Clear();
            sessionBytes[6] = 0x40;
            sessionBytes[8] = 0x80;
            BinaryPrimitives.WriteUInt32BigEndian(sessionBytes[12..], (uint)i);
            if (!SessionId.TryRead(sessionBytes, out var sessionId))
                throw new InvalidOperationException("The deterministic session fixture is invalid.");

            for (int j = 0; j < nonceBytes.Length; j++)
                nonceBytes[j] = (byte)(i + j);

            if (!Nonce256.TryRead(nonceBytes, out var nonce))
                throw new InvalidOperationException("The deterministic nonce fixture is invalid.");

            records[i] = new(sessionId, (ulong)i, nonce);
        }

        return records;
    }

    public static PresenceProofRecord[] Proofs(int count)
    {
        var records = new PresenceProofRecord[count];
        Span<byte> indexBytes = stackalloc byte[sizeof(uint)];
        Span<byte> signatureBytes = stackalloc byte[P256Signature.Size];

        for (int i = 0; i < records.Length; i++)
        {
            BinaryPrimitives.WriteUInt32BigEndian(indexBytes, (uint)i);
            var ticketHash = Hash256.Compute(indexBytes);
            for (int j = 0; j < signatureBytes.Length; j++)
                signatureBytes[j] = (byte)(i * 17 + j + 1);

            if (!P256Signature.TryRead(signatureBytes, out var signature))
                throw new InvalidOperationException("The deterministic signature fixture is invalid.");

            records[i] = new(ticketHash, signature);
        }

        return records;
    }
}
