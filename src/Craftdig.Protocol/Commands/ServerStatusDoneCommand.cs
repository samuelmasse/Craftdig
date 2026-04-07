namespace Craftdig.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ServerStatusDoneCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.ServerStatusDone;
}
