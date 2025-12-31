namespace Craftdig.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ChunkUpdateCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.ChunkUpdate;

    public Vector2i Cloc;
}
