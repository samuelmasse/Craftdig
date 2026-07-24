namespace Craftdig.Protocol;

public readonly record struct PresenceProofRecord(Hash256 TicketHash, P256Signature Signature) : IWireValue<PresenceProofRecord>
{
    public const int Size = Hash256.Size + P256Signature.Size;

    public static int WireSize => Size;

    public static bool TryRead(ReadOnlySpan<byte> source, out PresenceProofRecord record)
    {
        var reader = new SpanReader(source);
        record = new(reader.Read<Hash256>(), reader.Read<P256Signature>());
        if (reader.Finish())
            return true;

        record = default;
        return false;
    }

    public bool TryWrite(Span<byte> destination)
    {
        var writer = new SpanWriter(destination);
        writer.Write(TicketHash);
        writer.Write(Signature);
        return writer.Finish(out _);
    }
}
