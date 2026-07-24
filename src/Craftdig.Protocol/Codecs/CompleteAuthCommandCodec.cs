namespace Craftdig.Protocol;

public static class CompleteAuthCommandCodec
{
    public const int SignatureSize = P256Signature.Size;
    public const int MaxSize = ProtocolLimits.MaxIdentityTicketSize + SignatureSize;

    public static bool TryRead(
        ReadOnlySpan<byte> source,
        out ReadOnlySpan<byte> ticket,
        out P256Signature signature)
    {
        ticket = default;
        signature = default;
        if (source.Length <= SignatureSize || source.Length > MaxSize)
            return false;

        int ticketLength = source.Length - SignatureSize;
        return PlayerIdentityCommandCodec.TryRead(source[..ticketLength], out ticket) &&
            P256Signature.TryRead(source[ticketLength..], out signature);
    }

    public static bool TryWrite(
        Span<byte> destination,
        ReadOnlySpan<byte> ticket,
        P256Signature signature,
        out int written)
    {
        written = 0;
        int size = ticket.Length + SignatureSize;
        if (size > MaxSize || destination.Length < size ||
            !PlayerIdentityCommandCodec.TryWrite(destination, ticket, out int ticketWritten) ||
            !signature.TryWrite(destination[ticketWritten..]))
            return false;

        written = size;
        return true;
    }
}
