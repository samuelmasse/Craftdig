namespace Crafthoe.Player;

[Player]
public class PlayerContext(
    RootMouse mouse,
    RootKeyboard keyboard,
    RootCanvas canvas,
    RootQuadIndexBuffer quadIndexBuffer,
    RootPositionColorProgram3D positionColorProgram3D,
    PlayerGlw gl,
    PlayerPerspective perspective,
    PlayerCamera camera)
{
    private int vao;
    private int count;

    public void Load()
    {
        Vector3 red = (1, 0, 0);
        Vector3 green = (0, 1, 0);
        Vector3 blue = (0, 0, 1);
        Vector3 yellow = (1, 0, 1);

        PositionColorVertex[] vertices = [
            // Front
            new((0, 1, 1), red),
            new((1, 1, 1), green),
            new((0, 0, 1), blue),
            new((1, 0, 1), yellow),
            // Back
            new((1, 1, 0), red),
            new((0, 1, 0), green),
            new((1, 0, 0), blue),
            new((0, 0, 0), yellow),
            // Top
            new((0, 1, 0), red * 0.8f),
            new((1, 1, 0), green * 0.8f),
            new((0, 1, 1), blue * 0.8f),
            new((1, 1, 1), yellow * 0.8f),
            // Bottom
            new((0, 0, 1), red * 0.8f),
            new((1, 0, 1), green * 0.8f),
            new((0, 0, 0), blue * 0.8f),
            new((1, 0, 0), yellow * 0.8f),
            // Left
            new((0, 1, 0), red * 0.5f),
            new((0, 1, 1), green * 0.5f),
            new((0, 0, 0), blue * 0.5f),
            new((0, 0, 1), yellow * 0.5f),
            // Right
            new((1, 1, 1), red * 0.5f),
            new((1, 1, 0), green * 0.5f),
            new((1, 0, 1), blue * 0.5f),
            new((1, 0, 0), yellow * 0.5f)
        ];

        count = (vertices.Length / 4) * 6;

        vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);
        gl.BindBuffer(BufferTarget.ArrayBuffer, gl.GenBuffer());
        gl.BindBuffer(BufferTarget.ElementArrayBuffer, quadIndexBuffer.Id);
        gl.BufferData(BufferTarget.ArrayBuffer, vertices.AsSpan(), BufferUsageHint.StaticDraw);
        positionColorProgram3D.SetAttributes();
        gl.UnbindVertexArray();
        gl.UnbindBuffer(BufferTarget.ArrayBuffer);
        gl.UnbindBuffer(BufferTarget.ElementArrayBuffer);

        camera.Offset = (0, 0, 10);
        quadIndexBuffer.EnsureCapacity(vertices.Length);
    }

    public void Update(double time)
    {
        float speed = (float)(time * 10);

        if (keyboard.IsKeyDown(Keys.W))
            camera.Offset += camera.Front * speed;
        if (keyboard.IsKeyDown(Keys.A))
            camera.Offset -= camera.Right * speed;
        if (keyboard.IsKeyDown(Keys.S))
            camera.Offset -= camera.Front * speed;
        if (keyboard.IsKeyDown(Keys.D))
            camera.Offset += camera.Right * speed;

        if (keyboard.IsKeyDown(Keys.Space))
            camera.Offset.Y += speed;
        if (keyboard.IsKeyDown(Keys.LeftControl))
            camera.Offset.Y -= speed;

        mouse.Track = true;
        camera.Rotate(-mouse.Delta / 300);
        camera.PreventBackFlipsAndFrontFlips();
        camera.ComputeVectors();
        perspective.ComputeMatrix(canvas.Size, camera);
    }

    public void Render()
    {
        gl.Viewport(canvas.Size);
        gl.Enable(EnableCap.DepthTest);
        gl.DepthFunc(DepthFunction.Less);
        gl.Enable(EnableCap.CullFace);
        gl.CullFace(TriangleFace.Back);

        gl.UseProgram(positionColorProgram3D.Id);
        positionColorProgram3D.View = perspective.View;
        positionColorProgram3D.Projection = perspective.Projection;
        gl.BindVertexArray(vao);
        gl.DrawElements(BeginMode.Triangles, count, DrawElementsType.UnsignedInt, 0);
        gl.UnbindVertexArray();
        gl.UnuseProgram();

        gl.ResetCullFace();
        gl.Disable(EnableCap.CullFace);
        gl.ResetDepthFunc();
        gl.Disable(EnableCap.DepthTest);
        gl.ResetViewport();
    }
}
