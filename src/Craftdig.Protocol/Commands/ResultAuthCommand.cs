namespace Craftdig;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ResultAuthCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.ResultAuth;
}
