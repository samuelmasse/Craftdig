namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct WorldIndicesUpdateCommand : ICommand
{
    public static int CommandId => (int)Commands.WorldIndicesUpdate;
}
