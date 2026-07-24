namespace Craftdig.Protocol;

public readonly record struct PresenceChallengeRecord(SessionId SessionId, ulong Sequence, Nonce256 Nonce) :
    IComparable<PresenceChallengeRecord>, IWireValue<PresenceChallengeRecord>
{
    public const int Size = SessionId.Size + sizeof(ulong) + Nonce256.Size;

    public static int WireSize => Size;

    public static bool TryRead(ReadOnlySpan<byte> source, out PresenceChallengeRecord record)
    {
        var reader = new SpanReader(source);
        record = new(reader.Read<SessionId>(), reader.ReadUInt64(), reader.Read<Nonce256>());
        if (reader.Finish())
            return true;

        record = default;
        return false;
    }

    public bool TryWrite(Span<byte> destination)
    {
        var writer = new SpanWriter(destination);
        writer.Write(SessionId);
        writer.WriteUInt64(Sequence);
        writer.Write(Nonce);
        return writer.Finish(out _);
    }

    public int CompareTo(PresenceChallengeRecord other)
    {
        int result = SessionId.CompareTo(other.SessionId);
        if (result != 0)
            return result;

        result = Sequence.CompareTo(other.Sequence);
        return result != 0 ? result : Nonce.CompareTo(other.Nonce);
    }
}
