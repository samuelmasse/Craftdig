namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ForgetChunkCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.ForgetChunk;

    public Vector2i Cloc;
}
