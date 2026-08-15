namespace Craftdig;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BeginAuthCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.BeginAuth;
}
