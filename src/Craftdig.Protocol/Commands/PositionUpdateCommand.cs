namespace Craftdig;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PositionUpdateCommand : ICommand
{
    public static ushort CommandId => (ushort)Commands.PositionUpdate;

    public Vec3d Position;
    public Vec3d Velocity;
    public Vec3 LookAt;
    [MarshalAs(UnmanagedType.I1)]
    public bool IsFlying;
    [MarshalAs(UnmanagedType.I1)]
    public bool IsSprinting;
}
