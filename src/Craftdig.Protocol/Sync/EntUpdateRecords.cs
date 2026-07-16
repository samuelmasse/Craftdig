namespace Craftdig.Protocol;

[Flags]
public enum EntCreateFlags : byte
{
    None,
    Owner = 1
}

[Flags]
public enum EntStateFlags : byte
{
    None,
    Full = 1
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct EntCreateRecord(Guid id, EntCreateFlags flags)
{
    public readonly Guid Id = id;
    public readonly EntCreateFlags Flags = flags;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct EntStateRecord(Guid id, EntStateFlags flags, ushort componentCount)
{
    public readonly Guid Id = id;
    public readonly EntStateFlags Flags = flags;
    public readonly ushort ComponentCount = componentCount;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct EntDeleteRecord(Guid id)
{
    public readonly Guid Id = id;
}

public readonly record struct EntComponentToken(ushort Ordinal, bool IsPresent);
