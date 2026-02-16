namespace Craftdig.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CompleteAuthCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.CompleteAuth;
}
