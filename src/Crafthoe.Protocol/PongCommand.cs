namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PongCommand
{
    public PingCommand Ping;
}
