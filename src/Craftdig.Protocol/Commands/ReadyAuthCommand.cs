namespace Craftdig;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ReadyAuthCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.ReadyAuth;
}
