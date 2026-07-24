namespace Craftdig.Protocol;

public readonly struct P256Signature : IEquatable<P256Signature>, IWireValue<P256Signature>
{
    public const int Size = 64;

    public static int WireSize => Size;

    private readonly Hash256 r;
    private readonly Hash256 s;

    private P256Signature(Hash256 r, Hash256 s)
    {
        this.r = r;
        this.s = s;
    }

    public static bool TryRead(ReadOnlySpan<byte> source, out P256Signature signature)
    {
        if (source.Length != Size ||
            !Hash256.TryRead(source[..Hash256.Size], out var r) ||
            !Hash256.TryRead(source[Hash256.Size..], out var s))
        {
            signature = default;
            return false;
        }

        signature = new(r, s);
        return true;
    }

    public static P256Signature SignHash(ECDsa key, Hash256 digest)
    {
        Span<byte> digestBytes = stackalloc byte[Hash256.Size];
        Span<byte> signatureBytes = stackalloc byte[Size];
        digest.TryWrite(digestBytes);

        if (!key.TrySignHash(
                digestBytes,
                signatureBytes,
                DSASignatureFormat.IeeeP1363FixedFieldConcatenation,
                out int written) || written != Size || !TryRead(signatureBytes, out var signature))
            throw new CryptographicException("P-256 did not produce a 64-byte P1363 signature.");

        return signature;
    }

    public bool VerifyHash(ECDsa key, Hash256 digest)
    {
        Span<byte> digestBytes = stackalloc byte[Hash256.Size];
        Span<byte> signatureBytes = stackalloc byte[Size];
        digest.TryWrite(digestBytes);
        TryWrite(signatureBytes);
        return key.VerifyHash(digestBytes, signatureBytes, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
    }

    public bool TryWrite(Span<byte> destination)
    {
        if (destination.Length < Size)
            return false;

        return r.TryWrite(destination) && s.TryWrite(destination[Hash256.Size..]);
    }

    public bool Equals(P256Signature other) => r.Equals(other.r) && s.Equals(other.s);
    public override bool Equals(object? obj) => obj is P256Signature other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(r, s);

    public static bool operator ==(P256Signature left, P256Signature right) => left.Equals(right);
    public static bool operator !=(P256Signature left, P256Signature right) => !left.Equals(right);
}
