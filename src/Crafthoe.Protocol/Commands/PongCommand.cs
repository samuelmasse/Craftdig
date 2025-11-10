namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PongCommand : ICommand
{
    public static int CommandId => (int)Commands.Pong;

    public PingCommand Ping;
}
