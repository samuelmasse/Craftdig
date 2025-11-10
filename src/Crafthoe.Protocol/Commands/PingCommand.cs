namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PingCommand : ICommand
{
    public static int CommandId => (int)Commands.Ping;

    public long Timestamp;
    public long Id;
}
