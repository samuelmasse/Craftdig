namespace Craftdig.World.Backend;

public abstract class WorldComponentWriter
{
    public abstract bool Has(Ent ent);
    public abstract int Measure(Ent ent);
    public abstract void Write(Ent ent, Span<byte> buffer);
    public abstract void Read(EntMutIdx ent, ReadOnlySpan<byte> buffer);
    public abstract void WritePloc(EntMutIdx ent, ComponentPloc? index);
    public abstract ComponentPloc? ReadPloc(Ent ent);
}

[World]
public class WorldComponentWriter<T, N>() : WorldComponentWriter where T : unmanaged
{
    public override bool Has(Ent ent) => ent.Has<T, N>();

    public override int Measure(Ent ent) => Unsafe.SizeOf<T>();

    public override void Write(Ent ent, Span<byte> buffer)
    {
        var value = ent.Get<T, N>();
        MemoryMarshal.Write(buffer, in value);
    }

    public override void Read(EntMutIdx ent, ReadOnlySpan<byte> buffer)
    {
        var value = MemoryMarshal.Read<T>(buffer);
        ent.Set<T, N>(value);
    }

    public override void WritePloc(EntMutIdx ent, ComponentPloc? index) =>
        ent.Set<ComponentPloc?, WorldComponentWriter<T, N>>(index);
    public override ComponentPloc? ReadPloc(Ent ent) =>
        ent.Get<ComponentPloc?, WorldComponentWriter<T, N>>();
}

[World]
public class WorldComponentEntWriter<N>(WorldModuleIndices indices) : WorldComponentWriter
{
    public override bool Has(Ent ent) => ent.Has<Ent, N>();

    public override int Measure(Ent ent)
    {
        var val = ent.Get<Ent, N>();
        if (val.ModuleName != null)
            return sizeof(int);
        else return Unsafe.SizeOf<Guid>();
    }

    public override void Write(Ent ent, Span<byte> buffer)
    {
        var val = ent.Get<Ent, N>();
        if (val.ModuleName != null)
            MemoryMarshal.Write(buffer, indices[val]);
        else MemoryMarshal.Write(buffer, val.Id);
    }

    public override void Read(EntMutIdx ent, ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length == sizeof(int))
        {
            int index = MemoryMarshal.Read<int>(buffer);
            ent.Set<Ent, N>(indices[index]);
        }
        else
        {
            // TODO
        }
    }

    public override void WritePloc(EntMutIdx ent, ComponentPloc? index) =>
        ent.Set<ComponentPloc?, WorldComponentEntWriter<N>>(index);
    public override ComponentPloc? ReadPloc(Ent ent) =>
        ent.Get<ComponentPloc?, WorldComponentEntWriter<N>>();
}

[World]
public class WorldComponentEntMutIdxWriter<N>(WorldEntIndex entIndex) : WorldComponentWriter
{
    public override bool Has(Ent ent) => ent.Has<EntMutIdx, N>();

    public override int Measure(Ent ent) => Unsafe.SizeOf<Guid>();

    public override void Write(Ent ent, Span<byte> buffer)
    {
        var val = ent.Get<EntMutIdx, N>();
        MemoryMarshal.Write(buffer, val.Id);
    }

    public override void Read(EntMutIdx ent, ReadOnlySpan<byte> buffer)
    {
        var id = MemoryMarshal.Read<Guid>(buffer);
        ent.Set<EntMutIdx, N>(entIndex[id]);
    }

    public override void WritePloc(EntMutIdx ent, ComponentPloc? index) =>
        ent.Set<ComponentPloc?, WorldComponentEntMutIdxWriter<N>>(index);
    public override ComponentPloc? ReadPloc(Ent ent) =>
        ent.Get<ComponentPloc?, WorldComponentEntMutIdxWriter<N>>();
}

[World]
public class WorldComponentArrayWriter<T, N>() : WorldComponentWriter where T : unmanaged
{
    public override bool Has(Ent ent) => ent.Has<T[]?, N>();

    public override int Measure(Ent ent)
    {
        var values = ent.Get<T[]?, N>();
        return sizeof(int) + (values?.Length ?? 0) * Unsafe.SizeOf<T>();
    }

    public override void Write(Ent ent, Span<byte> buffer)
    {
        var values = ent.Get<T[]?, N>();

        if (values != null)
        {
            MemoryMarshal.Write(buffer, values.Length);
            MemoryMarshal.AsBytes(values.AsSpan()).CopyTo(buffer[sizeof(int)..]);
        }
        else MemoryMarshal.Write(buffer, -1);
    }

    public override void Read(EntMutIdx ent, ReadOnlySpan<byte> buffer)
    {
        int count = MemoryMarshal.Read<int>(buffer);
        if (count >= 0)
        {
            var values = new T[count];
            var bytes = MemoryMarshal.AsBytes(values.AsSpan());
            buffer[sizeof(int)..].CopyTo(bytes);
            ent.Set<T[]?, N>(values);
        }
        else ent.Set<T[]?, N>(null);
    }

    public override void WritePloc(EntMutIdx ent, ComponentPloc? index) =>
        ent.Set<ComponentPloc?, WorldComponentArrayWriter<T, N>>(index);
    public override ComponentPloc? ReadPloc(Ent ent) =>
        ent.Get<ComponentPloc?, WorldComponentArrayWriter<T, N>>();
}
