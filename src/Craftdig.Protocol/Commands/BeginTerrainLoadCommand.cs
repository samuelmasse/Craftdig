namespace Craftdig;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BeginTerrainLoadCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.BeginTerrainLoad;

    public Vec2i CenterCloc;
    public byte SideLength;
}
