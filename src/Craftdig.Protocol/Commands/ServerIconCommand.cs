namespace Craftdig;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ServerIconCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.ServerIcon;
}
