namespace Craftdig;

public static class UuidCodec
{
    public const int Size = 16;

    public static bool TryRead(ReadOnlySpan<byte> source, out Guid value)
    {
        if (source.Length != Size)
        {
            value = default;
            return false;
        }

        value = new(source, bigEndian: true);
        return true;
    }

    public static bool TryWrite(Guid value, Span<byte> destination) =>
        destination.Length >= Size && value.TryWriteBytes(destination, bigEndian: true, out int written) && written == Size;
}
