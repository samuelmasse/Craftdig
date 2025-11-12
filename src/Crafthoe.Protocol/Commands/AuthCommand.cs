namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct AuthCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.Auth;
}
