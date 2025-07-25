namespace Crafthoe.Player;

[Player]
public class PlayerPerspective(RootCanvas canvas, PlayerCamera camera)
{
    private float fov;
    private Matrix4 view;
    private Matrix4 projection;

    public ref float Fov => ref fov;
    public ref Matrix4 View => ref view;
    public ref Matrix4 Projection => ref projection;

    public void Update()
    {
        view = Matrix4.LookAt(camera.Offset, camera.Offset + camera.LookAt, camera.Up);

        float fovRad = MathHelper.DegreesToRadians(fov);
        float aspectRatio = canvas.Size.X / canvas.Size.Y;
        projection = Matrix4.CreatePerspectiveFieldOfView(fovRad, aspectRatio, 1 / 8f, 4096);
    }
}
