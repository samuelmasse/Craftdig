namespace Craftdig.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EntUpdateCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.EntUpdate;
}
