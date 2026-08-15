namespace Craftdig;

public static class ReadyAuthCommandCodec
{
    public const int Size = Nonce256.Size;

    public static bool TryRead(ReadOnlySpan<byte> source, out Nonce256 serverNonce) =>
        Nonce256.TryRead(source, out serverNonce);

    public static bool TryWrite(Span<byte> destination, Nonce256 serverNonce, out int written)
    {
        var writer = new SpanWriter(destination);
        writer.Write(serverNonce);
        return writer.Finish(out written);
    }
}
