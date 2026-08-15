namespace Craftdig;

public ref struct SpanReader(ReadOnlySpan<byte> source)
{
    // Failure slices only feed reads that happen after Failed is latched; their values are never observed by callers that check Finish.
    private static readonly byte[] failure = new byte[128];

    private ReadOnlySpan<byte> remaining = source;
    private bool failed;

    public readonly bool Failed => failed;

    public byte ReadByte() => Take(1)[0];

    public ushort ReadUInt16() => BinaryPrimitives.ReadUInt16BigEndian(Take(sizeof(ushort)));

    public uint ReadUInt32() => BinaryPrimitives.ReadUInt32BigEndian(Take(sizeof(uint)));

    public ulong ReadUInt64() => BinaryPrimitives.ReadUInt64BigEndian(Take(sizeof(ulong)));

    public T Read<T>() where T : struct, IWireValue<T>
    {
        if (T.TryRead(Take(T.WireSize), out T value))
            return value;

        failed = true;
        return default;
    }

    public ReadOnlySpan<byte> ReadRest()
    {
        var rest = remaining;
        remaining = default;
        return rest;
    }

    public bool Finish()
    {
        failed |= !remaining.IsEmpty;
        return !failed;
    }

    private ReadOnlySpan<byte> Take(int count)
    {
        if (!failed && remaining.Length >= count)
        {
            var slice = remaining[..count];
            remaining = remaining[count..];
            return slice;
        }

        failed = true;
        return failure.AsSpan(0, Math.Min(count, failure.Length));
    }
}
