namespace Craftdig;

public ref struct EntUpdateValidator(ReadOnlySpan<byte> data)
{
    private readonly ReadOnlySpan<byte> data = data;
    private int length;

    public readonly bool Complete => length == data.Length;

    public bool TryReadCreate(out EntCreateRecord record) => TryRead(out record);

    public bool TryReadState(out EntStateRecord record)
    {
        if (!TryRead(out record))
            return false;

        return true;
    }

    public bool TryReadComponent(out EntComponentToken token)
    {
        if (data.Length - length < sizeof(ushort))
        {
            token = default;
            return false;
        }

        ushort value = BinaryPrimitives.ReadUInt16LittleEndian(data[length..]);
        length += sizeof(ushort);
        token = new((ushort)(value >> 1), (value & 1) != 0);
        return true;
    }

    public bool TryReadPresent(EntSyncComponent component, IEntSyncReadValidator validator)
    {
        if (!component.TryValidate(data[length..], validator, out int size))
            return false;

        length += size;
        return true;
    }

    public bool TryReadDelete(out EntDeleteRecord record) => TryRead(out record);

    private bool TryRead<T>(out T value) where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        if (data.Length - length < size)
        {
            value = default;
            return false;
        }

        value = MemoryMarshal.Read<T>(data[length..]);
        length += size;
        return true;
    }
}
