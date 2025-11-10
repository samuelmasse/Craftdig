namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpawnPlayerCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.SpawnPlayer;
}
