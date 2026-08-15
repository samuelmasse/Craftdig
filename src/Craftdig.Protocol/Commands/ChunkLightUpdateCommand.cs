namespace Craftdig;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ChunkLightUpdateCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.ChunkLightUpdate;

    public Vec2i Cloc;
    public uint SectionMask;
    public byte Full;
}
