namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PingCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.Ping;

    public long Timestamp;
    public long Id;
}
