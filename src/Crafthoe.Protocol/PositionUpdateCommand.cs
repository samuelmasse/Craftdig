namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PositionUpdateCommand
{
    public Vector3d Position;
    public Vector3d Velocity;
    [MarshalAs(UnmanagedType.I1)]
    public bool IsFlying;
    [MarshalAs(UnmanagedType.I1)]
    public bool IsSprinting;
}
