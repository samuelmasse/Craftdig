namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ForgetSectionCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.ForgetSection;

    public Vector3i Sloc;
}
