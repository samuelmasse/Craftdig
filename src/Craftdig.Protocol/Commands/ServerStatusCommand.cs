namespace Craftdig;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ServerStatusCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.ServerStatus;

    public long Timestamp;
    public long DescriptionHash;
    public long IconHash;
}
