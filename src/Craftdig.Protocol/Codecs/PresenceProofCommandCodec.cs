namespace Craftdig;

public static class PresenceProofCommandCodec
{
    public const int Size = Hash256.Size + Hash256.Size + P256Signature.Size;

    public static bool TryRead(ReadOnlySpan<byte> source, out PresenceProof proof)
    {
        var reader = new SpanReader(source);
        proof = new(reader.Read<Hash256>(), reader.Read<Hash256>(), reader.Read<P256Signature>());
        if (reader.Finish())
            return true;

        proof = default;
        return false;
    }

    public static bool TryWrite(Span<byte> destination, PresenceProof proof, out int written)
    {
        var writer = new SpanWriter(destination);
        writer.Write(proof.RoundHash);
        writer.Write(proof.TicketHash);
        writer.Write(proof.Signature);
        return writer.Finish(out written);
    }
}
