namespace Craftdig;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ForgetSectionCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.ForgetSection;

    public Vec3i Sloc;
}
