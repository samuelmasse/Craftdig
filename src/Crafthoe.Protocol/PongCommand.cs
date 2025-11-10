namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential)]
public struct PongCommand
{
    public PingCommand Ping;
}
