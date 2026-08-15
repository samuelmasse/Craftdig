namespace Craftdig;

public readonly struct Nonce256 : IEquatable<Nonce256>, IComparable<Nonce256>, IWireValue<Nonce256>
{
    public const int Size = Hash256.Size;

    public static int WireSize => Size;

    private readonly Hash256 value;

    private Nonce256(Hash256 value) => this.value = value;

    public static Nonce256 CreateRandom()
    {
        Span<byte> bytes = stackalloc byte[Size];
        RandomNumberGenerator.Fill(bytes);
        TryRead(bytes, out var nonce);
        return nonce;
    }

    public static bool TryRead(ReadOnlySpan<byte> source, out Nonce256 nonce)
    {
        if (!Hash256.TryRead(source, out var value))
        {
            nonce = default;
            return false;
        }

        nonce = new(value);
        return true;
    }

    public bool TryWrite(Span<byte> destination) => value.TryWrite(destination);
    public int CompareTo(Nonce256 other) => value.CompareTo(other.value);
    public bool Equals(Nonce256 other) => value.Equals(other.value);
    public override bool Equals(object? obj) => obj is Nonce256 other && Equals(other);
    public override int GetHashCode() => value.GetHashCode();
    public override string ToString() => value.ToString();

    public static bool operator ==(Nonce256 left, Nonce256 right) => left.Equals(right);
    public static bool operator !=(Nonce256 left, Nonce256 right) => !left.Equals(right);
}
