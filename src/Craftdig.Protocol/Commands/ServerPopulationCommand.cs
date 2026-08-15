namespace Craftdig;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ServerPopulationCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.ServerPopulation;

    public int MaxPlayers;
    public int CurrentPlayers;
}
