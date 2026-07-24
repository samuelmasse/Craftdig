namespace Craftdig.Protocol;

public readonly struct Hash256 : IEquatable<Hash256>, IComparable<Hash256>, IWireValue<Hash256>
{
    public const int Size = 32;

    public static int WireSize => Size;

    private readonly ulong part0;
    private readonly ulong part1;
    private readonly ulong part2;
    private readonly ulong part3;

    private Hash256(ulong part0, ulong part1, ulong part2, ulong part3)
    {
        this.part0 = part0;
        this.part1 = part1;
        this.part2 = part2;
        this.part3 = part3;
    }

    public static Hash256 Compute(ReadOnlySpan<byte> source)
    {
        Span<byte> bytes = stackalloc byte[Size];
        SHA256.HashData(source, bytes);
        TryRead(bytes, out var value);
        return value;
    }

    public static bool TryRead(ReadOnlySpan<byte> source, out Hash256 value)
    {
        if (source.Length != Size)
        {
            value = default;
            return false;
        }

        value = new(
            BinaryPrimitives.ReadUInt64BigEndian(source),
            BinaryPrimitives.ReadUInt64BigEndian(source[8..]),
            BinaryPrimitives.ReadUInt64BigEndian(source[16..]),
            BinaryPrimitives.ReadUInt64BigEndian(source[24..]));
        return true;
    }

    public bool TryWrite(Span<byte> destination)
    {
        if (destination.Length < Size)
            return false;

        BinaryPrimitives.WriteUInt64BigEndian(destination, part0);
        BinaryPrimitives.WriteUInt64BigEndian(destination[8..], part1);
        BinaryPrimitives.WriteUInt64BigEndian(destination[16..], part2);
        BinaryPrimitives.WriteUInt64BigEndian(destination[24..], part3);
        return true;
    }

    public int CompareTo(Hash256 other)
    {
        int result = part0.CompareTo(other.part0);
        if (result != 0)
            return result;

        result = part1.CompareTo(other.part1);
        if (result != 0)
            return result;

        result = part2.CompareTo(other.part2);
        return result != 0 ? result : part3.CompareTo(other.part3);
    }

    public bool Equals(Hash256 other) =>
        part0 == other.part0 &&
        part1 == other.part1 &&
        part2 == other.part2 &&
        part3 == other.part3;

    public override bool Equals(object? obj) => obj is Hash256 other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(part0, part1, part2, part3);

    public override string ToString()
    {
        Span<byte> bytes = stackalloc byte[Size];
        TryWrite(bytes);
        return Convert.ToHexStringLower(bytes);
    }

    public static bool operator ==(Hash256 left, Hash256 right) => left.Equals(right);
    public static bool operator !=(Hash256 left, Hash256 right) => !left.Equals(right);
}
