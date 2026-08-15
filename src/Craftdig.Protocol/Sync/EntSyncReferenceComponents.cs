namespace Craftdig;

public class EntSyncModuleComponent<N>(EntComponent component, ushort ordinal, EntSyncAudience audience, int maximumCount) :
    EntSyncComponent(component, ordinal, audience, maximumCount)
{
    public override int ElementSize => sizeof(int);
    public override bool Has(Ent ent) => ent.Has<Ent, N>();

    public override int Measure(Ent ent) => sizeof(int);

    public override void Write(Ent ent, Span<byte> destination, IEntSyncWriteResolver resolver)
    {
        var value = ent.Get<Ent, N>();
        BinaryPrimitives.WriteInt32LittleEndian(destination, value == default ? -1 : resolver.ModuleIndex(value));
    }

    public override bool TryValidate(
        ReadOnlySpan<byte> source,
        IEntSyncReadValidator validator,
        out int size)
    {
        size = sizeof(int);
        if (source.Length < size)
            return false;

        int index = BinaryPrimitives.ReadInt32LittleEndian(source);
        return index == -1 || index >= 0 && validator.IsModuleIndexValid(index);
    }

    public override int Read(EntMutIdx ent, ReadOnlySpan<byte> source, IEntSyncReadResolver resolver)
    {
        int index = BinaryPrimitives.ReadInt32LittleEndian(source);
        ent.Set<Ent, N>(index == -1 ? default : resolver.ModuleEnt(index));
        return sizeof(int);
    }

    public override void Unset(EntMutIdx ent) => ent.Unset<Ent, N>();
}

public class EntSyncModuleArrayComponent<N>(EntComponent component, ushort ordinal, EntSyncAudience audience, int maximumCount) :
    EntSyncComponent(component, ordinal, audience, maximumCount)
{
    public override int ElementSize => sizeof(int);
    public override bool Has(Ent ent) => ent.Has<Ent[]?, N>();

    public override int Measure(Ent ent) => MeasureArray(ent.Get<Ent[]?, N>()?.Length, sizeof(int));

    public override void Write(Ent ent, Span<byte> destination, IEntSyncWriteResolver resolver)
    {
        var values = ent.Get<Ent[]?, N>();
        BinaryPrimitives.WriteInt32LittleEndian(destination, values?.Length ?? -1);
        if (values == null)
            return;

        for (int i = 0; i < values.Length; i++)
        {
            var value = values[i];
            BinaryPrimitives.WriteInt32LittleEndian(
                destination[(sizeof(int) + i * sizeof(int))..],
                value == default ? -1 : resolver.ModuleIndex(value));
        }
    }

    public override bool TryValidate(
        ReadOnlySpan<byte> source,
        IEntSyncReadValidator validator,
        out int size)
    {
        if (!TryMeasureArray(source, sizeof(int), out int count, out size))
            return false;

        for (int i = 0; i < count; i++)
        {
            int index = BinaryPrimitives.ReadInt32LittleEndian(source[(sizeof(int) + i * sizeof(int))..]);
            if (index != -1 && (index < 0 || !validator.IsModuleIndexValid(index)))
                return false;
        }

        return true;
    }

    public override int Read(EntMutIdx ent, ReadOnlySpan<byte> source, IEntSyncReadResolver resolver)
    {
        int count = BinaryPrimitives.ReadInt32LittleEndian(source);
        if (count == -1)
        {
            ent.Set<Ent[]?, N>(null);
            return sizeof(int);
        }

        var values = ent.Get<Ent[]?, N>();
        if (values == null || values.Length != count)
            values = new Ent[count];

        for (int i = 0; i < count; i++)
        {
            int index = BinaryPrimitives.ReadInt32LittleEndian(source[(sizeof(int) + i * sizeof(int))..]);
            values[i] = index == -1 ? default : resolver.ModuleEnt(index);
        }

        ent.Set<Ent[]?, N>(values);
        return sizeof(int) + count * sizeof(int);
    }

    public override void Unset(EntMutIdx ent) => ent.Unset<Ent[]?, N>();
}

public class EntSyncEntComponent<N>(EntComponent component, ushort ordinal, EntSyncAudience audience, int maximumCount) :
    EntSyncComponent(component, ordinal, audience, maximumCount)
{
    private const int EntIdSize = 16;

    public override int ElementSize => EntIdSize;
    public override bool Has(Ent ent) => ent.Has<EntMutIdx, N>();

    public override int Measure(Ent ent) => EntIdSize;

    public override void Write(Ent ent, Span<byte> destination, IEntSyncWriteResolver resolver)
    {
        var value = ent.Get<EntMutIdx, N>();
        var id = value == default ? Guid.Empty : resolver.EntId(value);
        MemoryMarshal.Write(destination, in id);
    }

    public override bool TryValidate(
        ReadOnlySpan<byte> source,
        IEntSyncReadValidator validator,
        out int size)
    {
        size = EntIdSize;
        if (source.Length < size)
            return false;

        var id = MemoryMarshal.Read<Guid>(source);
        return id == Guid.Empty || validator.IsEntIdValid(id);
    }

    public override int Read(EntMutIdx ent, ReadOnlySpan<byte> source, IEntSyncReadResolver resolver)
    {
        var id = MemoryMarshal.Read<Guid>(source);
        ent.Set<EntMutIdx, N>(id == Guid.Empty ? default : resolver.Ent(id));
        return EntIdSize;
    }

    public override void Unset(EntMutIdx ent) => ent.Unset<EntMutIdx, N>();
}

public class EntSyncEntArrayComponent<N>(EntComponent component, ushort ordinal, EntSyncAudience audience, int maximumCount) :
    EntSyncComponent(component, ordinal, audience, maximumCount)
{
    private const int EntIdSize = 16;

    public override int ElementSize => EntIdSize;
    public override bool Has(Ent ent) => ent.Has<EntMutIdx[]?, N>();

    public override int Measure(Ent ent) => MeasureArray(ent.Get<EntMutIdx[]?, N>()?.Length, EntIdSize);

    public override void Write(Ent ent, Span<byte> destination, IEntSyncWriteResolver resolver)
    {
        var values = ent.Get<EntMutIdx[]?, N>();
        BinaryPrimitives.WriteInt32LittleEndian(destination, values?.Length ?? -1);
        if (values == null)
            return;

        for (int i = 0; i < values.Length; i++)
        {
            var value = values[i];
            var id = value == default ? Guid.Empty : resolver.EntId(value);
            MemoryMarshal.Write(destination[(sizeof(int) + i * EntIdSize)..], in id);
        }
    }

    public override bool TryValidate(
        ReadOnlySpan<byte> source,
        IEntSyncReadValidator validator,
        out int size)
    {
        if (!TryMeasureArray(source, EntIdSize, out int count, out size))
            return false;

        for (int i = 0; i < count; i++)
        {
            var id = MemoryMarshal.Read<Guid>(source[(sizeof(int) + i * EntIdSize)..]);
            if (id != Guid.Empty && !validator.IsEntIdValid(id))
                return false;
        }

        return true;
    }

    public override int Read(EntMutIdx ent, ReadOnlySpan<byte> source, IEntSyncReadResolver resolver)
    {
        int count = BinaryPrimitives.ReadInt32LittleEndian(source);
        if (count == -1)
        {
            ent.Set<EntMutIdx[]?, N>(null);
            return sizeof(int);
        }

        var values = ent.Get<EntMutIdx[]?, N>();
        if (values == null || values.Length != count)
            values = new EntMutIdx[count];

        for (int i = 0; i < count; i++)
        {
            var id = MemoryMarshal.Read<Guid>(source[(sizeof(int) + i * EntIdSize)..]);
            values[i] = id == Guid.Empty ? default : resolver.Ent(id);
        }

        ent.Set<EntMutIdx[]?, N>(values);
        return sizeof(int) + count * EntIdSize;
    }

    public override void Unset(EntMutIdx ent) => ent.Unset<EntMutIdx[]?, N>();
}
