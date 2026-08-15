namespace Craftdig;

public readonly struct P256PublicKey : IEquatable<P256PublicKey>
{
    public const int CoordinateSize = Hash256.Size;

    public readonly Hash256 X;
    public readonly Hash256 Y;

    private P256PublicKey(Hash256 x, Hash256 y)
    {
        X = x;
        Y = y;
    }

    public static bool TryCreate(ReadOnlySpan<byte> xBytes, ReadOnlySpan<byte> yBytes, out P256PublicKey publicKey)
    {
        publicKey = default;
        if (!Hash256.TryRead(xBytes, out var x) || !Hash256.TryRead(yBytes, out var y))
            return false;

        var candidate = new P256PublicKey(x, y);
        try
        {
            using var key = ECDsa.Create(candidate.ExportParameters());
        }
        catch (Exception exception) when (exception is
            CryptographicException or
            ArgumentException or
            PlatformNotSupportedException)
        {
            return false;
        }

        publicKey = candidate;
        return true;
    }

    public ECParameters ExportParameters()
    {
        byte[] x = new byte[CoordinateSize];
        byte[] y = new byte[CoordinateSize];
        X.TryWrite(x);
        Y.TryWrite(y);
        return new()
        {
            Curve = ECCurve.NamedCurves.nistP256,
            Q = new()
            {
                X = x,
                Y = y,
            },
        };
    }

    public ECDsa CreateEcdsa() => ECDsa.Create(ExportParameters());

    public bool Equals(P256PublicKey other) => X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => obj is P256PublicKey other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y);

    public static bool operator ==(P256PublicKey left, P256PublicKey right) => left.Equals(right);
    public static bool operator !=(P256PublicKey left, P256PublicKey right) => !left.Equals(right);
}
