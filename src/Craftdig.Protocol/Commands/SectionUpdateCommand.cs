namespace Craftdig.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SectionUpdateCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.SectionUpdate;

    public Vector3i Sloc;
}
