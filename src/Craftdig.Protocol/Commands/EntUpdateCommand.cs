namespace Craftdig.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct EntUpdateCommand(uint scopeId, ushort createCount, ushort stateCount, ushort deleteCount) : ICommand
{
    public readonly uint ScopeId = scopeId;
    public readonly ushort CreateCount = createCount;
    public readonly ushort StateCount = stateCount;
    public readonly ushort DeleteCount = deleteCount;

    public static ushort CommandId => (ushort)Commands.EntUpdate;
}
