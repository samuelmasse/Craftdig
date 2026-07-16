namespace Craftdig.Protocol;

public class EntSyncUnmanagedComponent<T, N>(EntComponent component, ushort ordinal, EntSyncAudience audience, int maximumCount) :
    EntSyncComponent(component, ordinal, audience, maximumCount)
    where T : unmanaged
{
    public override int ElementSize => Unsafe.SizeOf<T>();
    public override bool Has(Ent ent) => ent.Has<T, N>();

    public override int Measure(Ent ent) => Unsafe.SizeOf<T>();

    public override void Write(Ent ent, Span<byte> destination, IEntSyncWriteResolver resolver)
    {
        var value = ent.Get<T, N>();
        MemoryMarshal.Write(destination, in value);
    }

    public override bool TryValidate(
        ReadOnlySpan<byte> source,
        IEntSyncReadValidator validator,
        out int size)
    {
        size = Unsafe.SizeOf<T>();
        return source.Length >= size;
    }

    public override int Read(EntMutIdx ent, ReadOnlySpan<byte> source, IEntSyncReadResolver resolver)
    {
        var value = MemoryMarshal.Read<T>(source);
        ent.Set<T, N>(value);
        return Unsafe.SizeOf<T>();
    }

    public override void Unset(EntMutIdx ent) => ent.Unset<T, N>();
}

public class EntSyncUnmanagedArrayComponent<T, N>(EntComponent component, ushort ordinal, EntSyncAudience audience, int maximumCount) :
    EntSyncComponent(component, ordinal, audience, maximumCount)
    where T : unmanaged
{
    public override int ElementSize => Unsafe.SizeOf<T>();
    public override bool Has(Ent ent) => ent.Has<T[]?, N>();

    public override int Measure(Ent ent)
    {
        var values = ent.Get<T[]?, N>();
        return MeasureArray(values?.Length, Unsafe.SizeOf<T>());
    }

    public override void Write(Ent ent, Span<byte> destination, IEntSyncWriteResolver resolver)
    {
        var values = ent.Get<T[]?, N>();
        BinaryPrimitives.WriteInt32LittleEndian(destination, values?.Length ?? -1);
        if (values != null)
            MemoryMarshal.AsBytes(values.AsSpan()).CopyTo(destination[sizeof(int)..]);
    }

    public override bool TryValidate(
        ReadOnlySpan<byte> source,
        IEntSyncReadValidator validator,
        out int size) =>
        TryMeasureArray(source, Unsafe.SizeOf<T>(), out _, out size);

    public override int Read(EntMutIdx ent, ReadOnlySpan<byte> source, IEntSyncReadResolver resolver)
    {
        int count = BinaryPrimitives.ReadInt32LittleEndian(source);
        if (count == -1)
        {
            ent.Set<T[]?, N>(null);
            return sizeof(int);
        }

        var values = ent.Get<T[]?, N>();
        if (values == null || values.Length != count)
            values = new T[count];

        int size = count * Unsafe.SizeOf<T>();
        source.Slice(sizeof(int), size).CopyTo(MemoryMarshal.AsBytes(values.AsSpan()));
        ent.Set<T[]?, N>(values);
        return sizeof(int) + size;
    }

    public override void Unset(EntMutIdx ent) => ent.Unset<T[]?, N>();
}
