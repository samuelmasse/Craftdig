namespace Crafthoe.Protocol;

[StructLayout(LayoutKind.Sequential)]
public struct PositionUpdateCommand
{
    public Vector3d Position;
    public bool IsFlying;
    public bool IsSprinting;
}
