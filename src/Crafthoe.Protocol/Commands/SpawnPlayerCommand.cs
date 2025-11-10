namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpawnPlayerCommand : ICommand
{
    public static int CommandId => (int)Commands.SpawnPlayer;
}
