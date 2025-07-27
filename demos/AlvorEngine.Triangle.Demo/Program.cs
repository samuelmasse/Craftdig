using AlvorEngine;
using AlvorEngine.Loop;
using GlwLayer.Driver.OpenTK;
using OpenTK.Graphics.OpenGL4;
using Wigdow;

new RootLoop(new()
{
    Window = new WindowOpenTK(new(new(), new() { StartVisible = false })),
    Driver = new GldOpenTK(),
    BootState = typeof(TriangleState)
}).Run();

[Root]
class TriangleState(
    RootGlw gl,
    RootScreen screen,
    RootBackbuffer backbuffer,
    RootCanvas canvas,
    RootPositionColorProgram positionColorProgram) : State
{
    private int vao;

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
        positionColorProgram.SetAttributes();
        gl.UnbindVertexArray();
        gl.UnbindBuffer(BufferTarget.ArrayBuffer);

        screen.Title = "AlvorEngine.Triangle.Demo";
        screen.Size = screen.MonitorSize / 4 * 3;
        screen.IsVisible = true;
    }

    public override void Render()
    {
        backbuffer.Clear();
        gl.Viewport(canvas.Size);
        gl.UseProgram(positionColorProgram.Id);
        gl.BindVertexArray(vao);
        gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
        gl.UnbindVertexArray();
        gl.UnuseProgram();
        gl.ResetViewport();
    }
}
