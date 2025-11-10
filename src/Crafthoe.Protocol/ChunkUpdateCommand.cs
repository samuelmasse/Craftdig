namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ChunkUpdateCommand
{
    public Vector2i Cloc;
}
