namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ForgetChunkCommand
{
    public Vector2i Cloc;
}
