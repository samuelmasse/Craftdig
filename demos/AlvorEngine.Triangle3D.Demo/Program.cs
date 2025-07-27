using AlvorEngine;
using AlvorEngine.Loop;
using Glw2D.Fonts;
using GlwLayer.Driver.OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Wigdow;

new RootLoop(new()
{
    Window = new WindowOpenTK(new(new(), new() { StartVisible = false })),
    Driver = new GldOpenTK(),
    BootState = typeof(Triangle3DState)
}).Run();

[Root]
class Triangle3DState(
    RootMouse mouse,
    RootKeyboard keyboard,
    RootGlw gl,
    RootScreen screen,
    RootBackbuffer backbuffer,
    RootCanvas canvas,
    RootPositionColorProgram3D positionColorProgram3D,
    RootSprites sprites,
    RootRoboto roboto,
    RootText text,
    RootScale scale) : State
{
    private readonly Camera3D camera = new();
    private readonly Perspective3D perspective = new();
    private int vao;
    private bool paused;

    public override void Load()
    {
        PositionColorVertex[] vertices =
        [
            new((0.5f, -0.5f, 0.0f), (1.0f, 0.0f, 0.0f)),
            new((-0.5f, -0.5f, 0.0f), (0.0f, 1.0f, 0.0f)),
            new((0.0f, 0.5f, 0.0f), (0.0f, 0.0f, 1.0f))
        ];

        vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);
        gl.BindBuffer(BufferTarget.ArrayBuffer, gl.GenBuffer());
        gl.BufferData(BufferTarget.ArrayBuffer, vertices.AsSpan(), BufferUsageHint.StaticDraw);
        positionColorProgram3D.SetAttributes();
        gl.UnbindVertexArray();
        gl.UnbindBuffer(BufferTarget.ArrayBuffer);

        camera.Offset = (0, 0, 10);

        screen.Title = "AlvorEngine.Triangle3D.Demo";
        screen.Size = screen.MonitorSize / 4 * 3;
        screen.IsVisible = true;
    }

    public override void Update(double time)
    {
        if (keyboard.IsKeyPressed(Keys.Escape))
            paused = !paused;

        mouse.Track = !paused;
        mouse.CursorState = paused ? CursorState.Normal : CursorState.Grabbed;

        if (!paused)
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
        }

        if (keyboard.IsKeyPressedRepeated(Keys.Minus) && scale.Numerator > scale.Denominator)
            scale.Numerator--;
        if (keyboard.IsKeyPressedRepeated(Keys.Equal) && scale.Numerator < scale.Denominator * 4)
            scale.Numerator++;

        camera.Rotate(-mouse.Delta / 300);
        camera.PreventBackFlipsAndFrontFlips();
        camera.ComputeVectors();
        perspective.ComputeMatrix(canvas.Size, camera);
    }

    public override void Render()
    {
        backbuffer.Clear();
        gl.Viewport(canvas.Size);
        gl.UseProgram(positionColorProgram3D.Id);
        positionColorProgram3D.View = perspective.View;
        positionColorProgram3D.Projection = perspective.Projection;
        gl.BindVertexArray(vao);
        gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
        gl.UnbindVertexArray();
        gl.UnuseProgram();
        gl.ResetViewport();
    }

    public override void Draw()
    {
        var topLeft = new Vector2(scale[25], scale[25]);
        sprites.Batch.Write(roboto[scale[27]], text.Format("Position: {0:F3}", camera.Offset), topLeft);
        sprites.Batch.Write(roboto[scale[27]], text.Format("Rotation: {0:F3}", camera.Rotation),
            topLeft + (0, roboto[scale[27]].Metrics.Height));

        if (paused)
            sprites.Batch.Draw((0, 0), canvas.Size, (0.3f, 0.3f, 0.3f, 0.3f));
    }
}
