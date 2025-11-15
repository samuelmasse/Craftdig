namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SlowTickCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.SlowTick;
}
