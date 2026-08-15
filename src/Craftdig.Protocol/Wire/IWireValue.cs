namespace Craftdig;

public interface IWireValue<TSelf> where TSelf : struct, IWireValue<TSelf>
{
    static abstract int WireSize { get; }

    static abstract bool TryRead(ReadOnlySpan<byte> source, out TSelf value);

    bool TryWrite(Span<byte> destination);
}
