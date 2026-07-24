namespace Craftdig.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct PresenceProofCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.PresenceProof;
}
