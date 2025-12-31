namespace Craftdig.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NoAuthCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.NoAuth;
}
