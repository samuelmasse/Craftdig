namespace AlvorEngine;

public class Perspective3D
{
    private float fov = 70;
    private float near = 0.1f;
    private float far = 1000f;
    private Matrix4 view;
    private Matrix4 projection;

    public ref float Fov => ref fov;
    public ref float Near => ref near;
    public ref float Far => ref far;
    public ref Matrix4 View => ref view;
    public ref Matrix4 Projection => ref projection;

    public void ComputeMatrix(Vector2 canvas, Camera3D camera)
    {
        float fovy = MathHelper.DegreesToRadians(fov);
        float aspect = canvas.X / canvas.Y;
        view = Matrix4.LookAt(camera.Offset, camera.Offset + camera.LookAt, camera.Up);
        projection = Matrix4.CreatePerspectiveFieldOfView(fovy, aspect, near, far);
    }
}
