namespace Craftdig.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct PlayerIdentityCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.PlayerIdentity;
}
