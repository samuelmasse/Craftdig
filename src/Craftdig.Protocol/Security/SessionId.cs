namespace Craftdig.Protocol;

public readonly struct SessionId : IEquatable<SessionId>, IComparable<SessionId>, IWireValue<SessionId>
{
    public const int Size = UuidCodec.Size;

    public static int WireSize => Size;

    private readonly ulong high;
    private readonly ulong low;

    private SessionId(ulong high, ulong low)
    {
        this.high = high;
        this.low = low;
    }

    public Guid Value
    {
        get
        {
            Span<byte> bytes = stackalloc byte[Size];
            TryWrite(bytes);
            UuidCodec.TryRead(bytes, out var value);
            return value;
        }
    }

    public static SessionId CreateRandom()
    {
        TryFromGuid(Guid.NewGuid(), out var sessionId);
        return sessionId;
    }

    public static bool TryFromGuid(Guid value, out SessionId sessionId)
    {
        Span<byte> bytes = stackalloc byte[Size];
        if (!UuidCodec.TryWrite(value, bytes))
        {
            sessionId = default;
            return false;
        }

        return TryRead(bytes, out sessionId);
    }

    public static bool TryRead(ReadOnlySpan<byte> source, out SessionId sessionId)
    {
        if (source.Length != Size || (source[6] & 0xf0) != 0x40 || (source[8] & 0xc0) != 0x80)
        {
            sessionId = default;
            return false;
        }

        sessionId = new(
            BinaryPrimitives.ReadUInt64BigEndian(source),
            BinaryPrimitives.ReadUInt64BigEndian(source[8..]));
        return true;
    }

    public bool TryWrite(Span<byte> destination)
    {
        if (destination.Length < Size)
            return false;

        BinaryPrimitives.WriteUInt64BigEndian(destination, high);
        BinaryPrimitives.WriteUInt64BigEndian(destination[8..], low);
        return true;
    }

    public int CompareTo(SessionId other)
    {
        int result = high.CompareTo(other.high);
        return result != 0 ? result : low.CompareTo(other.low);
    }

    public bool Equals(SessionId other) => high == other.high && low == other.low;
    public override bool Equals(object? obj) => obj is SessionId other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(high, low);
    public override string ToString() => Value.ToString("D");

    public static bool operator ==(SessionId left, SessionId right) => left.Equals(right);
    public static bool operator !=(SessionId left, SessionId right) => !left.Equals(right);
}
