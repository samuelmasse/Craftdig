namespace Craftdig.Player.Frontend;

[Player]
public class PlayerCamera : Camera3D
{
    public void SetLookAt(Vec3 lookAt)
    {
        if (lookAt == default)
            lookAt = (0, 0, -1);

        float pitch = -MathF.Asin(-lookAt.Y);
        float yaw = MathF.Atan2(-lookAt.X, -lookAt.Z);
        Rotation = (yaw, pitch, 0);
        ComputeVectors();
    }
}
