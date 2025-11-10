namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct WorldIndicesUpdateCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.WorldIndicesUpdate;
}
