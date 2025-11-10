namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PongCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.Pong;

    public PingCommand Ping;
}
