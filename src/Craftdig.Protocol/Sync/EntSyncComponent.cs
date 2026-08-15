namespace Craftdig;

public abstract class EntSyncComponent(
    EntComponent component,
    ushort ordinal,
    EntSyncAudience audience,
    int maximumCount)
{
    public EntComponent Component { get; } = component;
    public ushort Ordinal { get; } = ordinal;
    public EntSyncAudience Audience { get; } = audience;
    public int MaximumCount { get; } = maximumCount;
    public abstract int ElementSize { get; }

    public abstract bool Has(Ent ent);
    public abstract int Measure(Ent ent);
    public abstract void Write(Ent ent, Span<byte> destination, IEntSyncWriteResolver resolver);
    public abstract bool TryValidate(
        ReadOnlySpan<byte> source,
        IEntSyncReadValidator validator,
        out int size);
    public abstract int Read(EntMutIdx ent, ReadOnlySpan<byte> source, IEntSyncReadResolver resolver);
    public abstract void Unset(EntMutIdx ent);

    protected bool TryMeasureArray(
        ReadOnlySpan<byte> source,
        int elementSize,
        out int count,
        out int size)
    {
        count = 0;
        size = 0;
        if (source.Length < sizeof(int))
            return false;

        count = BinaryPrimitives.ReadInt32LittleEndian(source);
        if (count == -1)
        {
            size = sizeof(int);
            return true;
        }

        if (count < 0 || count > MaximumCount || count > (source.Length - sizeof(int)) / elementSize)
            return false;

        size = sizeof(int) + count * elementSize;
        return true;
    }

    protected int MeasureArray(int? count, int elementSize)
    {
        if (count > MaximumCount)
            throw new InvalidOperationException($"Component {Component} exceeds its maximum count of {MaximumCount}.");

        return sizeof(int) + count.GetValueOrDefault() * elementSize;
    }
}
