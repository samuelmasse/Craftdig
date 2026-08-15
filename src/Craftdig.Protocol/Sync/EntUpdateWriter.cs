namespace Craftdig;

public ref struct EntUpdateWriter(Span<byte> data)
{
    private readonly Span<byte> data = data;
    private int length;
    private ushort createCount;
    private ushort stateCount;
    private ushort deleteCount;

    public readonly int Length => length;
    private readonly int Remaining => data.Length - length;

    public bool TryWriteCreate(Guid id, EntCreateFlags flags = EntCreateFlags.None)
    {
        if (Remaining < Unsafe.SizeOf<EntCreateRecord>())
            return false;

        var record = new EntCreateRecord(id, flags);
        Write(record);
        createCount++;
        return true;
    }

    public bool TryStartState(Guid id, EntStateFlags flags, ushort componentCount, int componentBytes)
    {
        if (Remaining < Unsafe.SizeOf<EntStateRecord>() + componentBytes)
            return false;

        var record = new EntStateRecord(id, flags, componentCount);
        Write(record);
        stateCount++;
        return true;
    }

    public void WritePresent(EntSyncComponent component, Ent ent, IEntSyncWriteResolver resolver)
    {
        WriteToken(component.Ordinal, true);
        int size = component.Measure(ent);
        component.Write(ent, data[length..], resolver);
        length += size;
    }

    public void WriteAbsent(ushort ordinal) => WriteToken(ordinal, false);

    public bool TryWriteDelete(Guid id)
    {
        if (Remaining < Unsafe.SizeOf<EntDeleteRecord>())
            return false;

        var record = new EntDeleteRecord(id);
        Write(record);
        deleteCount++;
        return true;
    }

    public readonly EntUpdateCommand Command(uint scopeId) => new(scopeId, createCount, stateCount, deleteCount);

    private void WriteToken(ushort ordinal, bool present)
    {
        ushort token = (ushort)(ordinal << 1 | (present ? 1 : 0));
        BinaryPrimitives.WriteUInt16LittleEndian(data[length..], token);
        length += sizeof(ushort);
    }

    private void Write<T>(T value) where T : unmanaged
    {
        MemoryMarshal.Write(data[length..], in value);
        length += Unsafe.SizeOf<T>();
    }
}
