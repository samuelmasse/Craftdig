namespace Craftdig;

public static class PresenceChallengeCommandCodec
{
    public const int Size = sizeof(ulong) + Nonce256.Size;

    public static bool TryRead(ReadOnlySpan<byte> source, out PresenceChallenge challenge)
    {
        var reader = new SpanReader(source);
        challenge = new(reader.ReadUInt64(), reader.Read<Nonce256>());
        if (reader.Finish())
            return true;

        challenge = default;
        return false;
    }

    public static bool TryWrite(Span<byte> destination, PresenceChallenge challenge, out int written)
    {
        var writer = new SpanWriter(destination);
        writer.WriteUInt64(challenge.Sequence);
        writer.Write(challenge.Nonce);
        return writer.Finish(out written);
    }
}
