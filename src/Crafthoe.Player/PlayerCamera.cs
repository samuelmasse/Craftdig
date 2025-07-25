namespace Crafthoe.Player;

[Player]
public class PlayerCamera
{
    private Vector3 offset;
    private Vector3 lookAt;
    private Vector3 up;
    private float fov;

    public ref Vector3 Offset => ref offset;
    public ref Vector3 LookAt => ref lookAt;
    public ref Vector3 Up => ref up;
    public ref float Fov => ref fov;

    public void Update()
    {
        fov = 70;
        lookAt = (0, 0, -1);
        up = (0, 1, 0);
    }
}
