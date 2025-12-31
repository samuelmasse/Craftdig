namespace Craftdig.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SlowDownCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.SlowDown;
}
