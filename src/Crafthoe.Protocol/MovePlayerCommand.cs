namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MovePlayerCommand
{
    public MovementStep Step;
}
