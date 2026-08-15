namespace Craftdig;

public ref struct SpanWriter(Span<byte> destination)
{
    private readonly Span<byte> destination = destination;
    private int offset;
    private bool failed;

    public readonly bool Failed => failed;

    public void WriteByte(byte value)
    {
        if (TryAdvance(1, out var slice))
            slice[0] = value;
    }

    public void WriteUInt16(ushort value)
    {
        if (TryAdvance(sizeof(ushort), out var slice))
            BinaryPrimitives.WriteUInt16BigEndian(slice, value);
    }

    public void WriteUInt32(uint value)
    {
        if (TryAdvance(sizeof(uint), out var slice))
            BinaryPrimitives.WriteUInt32BigEndian(slice, value);
    }

    public void WriteUInt64(ulong value)
    {
        if (TryAdvance(sizeof(ulong), out var slice))
            BinaryPrimitives.WriteUInt64BigEndian(slice, value);
    }

    public void Write<T>(in T value) where T : struct, IWireValue<T>
    {
        if (TryAdvance(T.WireSize, out var slice) && !value.TryWrite(slice))
            failed = true;
    }

    public void WriteBytes(ReadOnlySpan<byte> bytes)
    {
        if (TryAdvance(bytes.Length, out var slice))
            bytes.CopyTo(slice);
    }

    public readonly bool Finish(out int written)
    {
        written = failed ? 0 : offset;
        return !failed;
    }

    private bool TryAdvance(int count, out Span<byte> slice)
    {
        if (!failed && destination.Length - offset >= count)
        {
            slice = destination.Slice(offset, count);
            offset += count;
            return true;
        }

        failed = true;
        slice = default;
        return false;
    }
}
