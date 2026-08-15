namespace Craftdig;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SectionUpdateCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.SectionUpdate;

    public Vec3i Sloc;
}
