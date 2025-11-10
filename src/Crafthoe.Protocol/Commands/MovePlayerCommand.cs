namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MovePlayerCommand : ICommand
{
    public static int CommandId => (int)Commands.MovePlayer;

    public MovementStep Step;
}
