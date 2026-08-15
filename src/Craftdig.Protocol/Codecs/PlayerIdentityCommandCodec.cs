namespace Craftdig;

public static class PlayerIdentityCommandCodec
{
    public const int MaxSize = ProtocolLimits.MaxIdentityTicketSize;

    public static bool TryRead(ReadOnlySpan<byte> source, out ReadOnlySpan<byte> ticket)
    {
        if (!IsCompactJwt(source))
        {
            ticket = default;
            return false;
        }

        ticket = source;
        return true;
    }

    public static bool TryWrite(Span<byte> destination, ReadOnlySpan<byte> ticket, out int written)
    {
        written = 0;
        if (destination.Length < ticket.Length || !IsCompactJwt(ticket))
            return false;

        ticket.CopyTo(destination);
        written = ticket.Length;
        return true;
    }

    public static bool IsCompactJwt(ReadOnlySpan<byte> ticket)
    {
        if (ticket.Length is < 5 or > MaxSize)
            return false;

        int dots = 0;
        int segmentLength = 0;
        foreach (byte value in ticket)
        {
            if (value == '.')
            {
                if (segmentLength == 0 || ++dots > 2)
                    return false;

                segmentLength = 0;
                continue;
            }

            bool base64Url = value is >= (byte)'A' and <= (byte)'Z' or
                >= (byte)'a' and <= (byte)'z' or
                >= (byte)'0' and <= (byte)'9' or
                (byte)'-' or (byte)'_';
            if (!base64Url)
                return false;

            segmentLength++;
        }

        return dots == 2 && segmentLength != 0;
    }
}
