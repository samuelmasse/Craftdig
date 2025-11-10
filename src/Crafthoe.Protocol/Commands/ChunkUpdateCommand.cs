namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ChunkUpdateCommand : ICommand
{
    public static int CommandId => (int)Commands.ChunkUpdate;

    public Vector2i Cloc;
}
