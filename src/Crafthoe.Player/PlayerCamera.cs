namespace Crafthoe.Player;

[Player]
public class PlayerCamera
{
    private Vector3 offset;
    private Vector3 rotation;

    private Vector3 lookAt;
    private Vector3 up;
    private Vector3 right;
    private Vector3 front;

    public ref Vector3 Offset => ref offset;
    public ref Vector3 Rotation => ref rotation;

    public Vector3 LookAt => lookAt;
    public Vector3 Front => front;
    public Vector3 Up => up;
    public Vector3 Right => right;

    public void Rotate(Vector3 delta)
    {
        rotation.X = RotateAngle(rotation.X, delta.X);
        rotation.Y = RotateAngle(rotation.Y, delta.Y);
        rotation.Z = RotateAngle(rotation.Z, delta.Z);
    }

    public void PreventBackFlipsAndFrontFlips()
    {
        if (rotation.Y < MathHelper.DegreesToRadians(270) && rotation.Y >= MathHelper.DegreesToRadians(180))
            rotation.Y = MathHelper.DegreesToRadians(270);
        else if (rotation.Y > MathHelper.DegreesToRadians(89.9f) && rotation.Y <= MathHelper.DegreesToRadians(180))
            rotation.Y = MathHelper.DegreesToRadians(89.9f);
    }

    public void Update()
    {
        lookAt = (0, 0, -1);
        up = (0, 1, 0);
        right = (1, 0, 0);
        front = (0, 0, -1);

        var yaw = Matrix4.CreateFromAxisAngle(up, rotation.X);
        lookAt = Vector3.TransformPerspective(lookAt, yaw);
        right = Vector3.TransformPerspective(right, yaw);
        front = Vector3.TransformPerspective(front, yaw);

        var pitch = Matrix4.CreateFromAxisAngle(right, rotation.Y);
        lookAt = Vector3.TransformPerspective(lookAt, pitch);
        up = Vector3.TransformPerspective(up, pitch);
    }

    private float RotateAngle(float angle, float delta)
    {
        angle += delta;

        if (angle > MathHelper.Pi * 2)
            angle -= MathHelper.Pi * 2;
        else if (angle < 0)
            angle += MathHelper.Pi * 2;

        return angle;
    }
}
