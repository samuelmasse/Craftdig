namespace Craftdig;

public ref struct EntUpdateReader(EntUpdateCommand command, ReadOnlySpan<byte> data)
{
    private readonly EntUpdateCommand command = command;
    private readonly ReadOnlySpan<byte> data = data;
    private int length;
    private ushort createsRead;
    private ushort statesRead;
    private ushort deletesRead;

    private readonly ReadOnlySpan<byte> Remaining => data[length..];

    public bool TryReadCreate(out EntCreateRecord record)
    {
        if (createsRead == command.CreateCount)
        {
            record = default;
            return false;
        }

        record = Read<EntCreateRecord>();
        createsRead++;
        return true;
    }

    public bool TryReadState(out EntStateRecord record)
    {
        if (statesRead == command.StateCount)
        {
            record = default;
            return false;
        }

        record = Read<EntStateRecord>();
        statesRead++;
        return true;
    }

    public EntComponentToken ReadComponent()
    {
        ushort token = BinaryPrimitives.ReadUInt16LittleEndian(Remaining);
        length += sizeof(ushort);
        return new((ushort)(token >> 1), (token & 1) != 0);
    }

    public void ReadPresent(EntSyncComponent component, EntMutIdx ent, IEntSyncReadResolver resolver) =>
        length += component.Read(ent, Remaining, resolver);

    public bool TryReadDelete(out EntDeleteRecord record)
    {
        if (deletesRead == command.DeleteCount)
        {
            record = default;
            return false;
        }

        record = Read<EntDeleteRecord>();
        deletesRead++;
        return true;
    }

    private T Read<T>() where T : unmanaged
    {
        var value = MemoryMarshal.Read<T>(Remaining);
        length += Unsafe.SizeOf<T>();
        return value;
    }
}
