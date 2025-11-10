namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MovePlayerCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.MovePlayer;

    public MovementStep Step;
}
