namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential)]
public struct PingCommand
{
    public long Timestamp;
    public long Id;
}
