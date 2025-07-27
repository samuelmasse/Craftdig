using AlvorEngine;
using AlvorEngine.Loop;
using Glw2D;
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
    RootQuadIndexBuffer quadIndexBuffer,
    RootPositionColorTextureProgram3D positionColorTextureProgram3D,
    RootCube cube,
    RootPngs pngs,
    RootSprites sprites,
    RootRoboto roboto,
    RootText text,
    RootScale scale,
    RootMetrics metrics) : State
{
    private readonly Camera3D camera = new();
    private readonly Perspective3D perspective = new();
    private Texture texture = null!;
    private int vao;
    private int count;
    private bool paused;

    public override void Load()
    {
        var image = pngs["Noise.png"];
        texture = new Texture2D(gl, image.Size)
        {
            PixelsMipmap = image.Pixels.Span,
            MagFilter = TextureMagFilter.Nearest,
            MinFilter = TextureMinFilter.NearestMipmapLinear
        };

        PositionColorTextureVertex[] vertices =
        [
            ..VertexQuad(cube.Front.Quad, 1),
            ..VertexQuad(cube.Back.Quad, 1),
            ..VertexQuad(cube.Top.Quad, 0.8f),
            ..VertexQuad(cube.Bottom.Quad, 0.8f),
            ..VertexQuad(cube.Left.Quad, 0.5f),
            ..VertexQuad(cube.Right.Quad, 0.5f)
        ];

        static PositionColorTextureVertex[] VertexQuad(Quad quad, float shadow) =>
        [
            new(quad.TopLeft, new Vector3(1, 0, 0) * shadow, (0, 1)),
            new(quad.TopRight, new Vector3(0, 1, 0) * shadow, (1, 1)),
            new(quad.BottomLeft, new Vector3(0, 0, 1) * shadow, (0, 0)),
            new(quad.BottomRight, new Vector3(1, 0, 1) * shadow, (1, 0))
        ];

        vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);
        gl.BindBuffer(BufferTarget.ArrayBuffer, gl.GenBuffer());
        gl.BindBuffer(BufferTarget.ElementArrayBuffer, quadIndexBuffer.Id);
        gl.BufferData(BufferTarget.ArrayBuffer, vertices.AsSpan(), BufferUsageHint.StaticDraw);
        positionColorTextureProgram3D.SetAttributes();
        gl.UnbindVertexArray();
        gl.UnbindBuffer(BufferTarget.ArrayBuffer);
        gl.UnbindBuffer(BufferTarget.ElementArrayBuffer);

        camera.Offset = (0, 0, 10);
        quadIndexBuffer.EnsureCapacity(vertices.Length);
        count = quadIndexBuffer.IndexCount(vertices.Length);

        screen.Title = "AlvorEngine.TextureCube.Demo";
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
    }

    public override void Render()
    {
        camera.ComputeVectors();
        perspective.ComputeMatrix(canvas.Size, camera);

        backbuffer.Clear();
        gl.Viewport(canvas.Size);
        gl.Enable(EnableCap.DepthTest);
        gl.DepthFunc(DepthFunction.Less);
        gl.Enable(EnableCap.CullFace);
        gl.CullFace(TriangleFace.Back);

        gl.UseProgram(positionColorTextureProgram3D.Id);
        positionColorTextureProgram3D.View = perspective.View;
        positionColorTextureProgram3D.Projection = perspective.Projection;
        texture.Bind(positionColorTextureProgram3D.SamplerTexture);
        gl.BindVertexArray(vao);
        gl.DrawElements(BeginMode.Triangles, count, DrawElementsType.UnsignedInt, 0);
        gl.UnbindVertexArray();
        texture.Unbind(positionColorTextureProgram3D.SamplerTexture);
        gl.UnuseProgram();

        gl.ResetCullFace();
        gl.Disable(EnableCap.CullFace);
        gl.ResetDepthFunc();
        gl.Disable(EnableCap.DepthTest);
        gl.ResetViewport();
    }

    public override void Draw()
    {
        var topLeft = new Vector2(scale[25], scale[25]);
        sprites.Batch.Write(roboto[scale[27]], text.Format("Frame: {0}. {1:F3} ms ({2} FPS)",
            metrics.Frame.Ticks, metrics.FrameWindow.Average, metrics.FrameWindow.Ticks), topLeft);
        sprites.Batch.Write(roboto[scale[27]], text.Format("Position: {0:F3}", camera.Offset),
            topLeft + (0, roboto[scale[27]].Metrics.Height));
        sprites.Batch.Write(roboto[scale[27]], text.Format("Rotation: {0:F3}", camera.Rotation),
            topLeft + (0, roboto[scale[27]].Metrics.Height * 2));

        if (paused)
            sprites.Batch.Draw((0, 0), canvas.Size, (0.3f, 0.3f, 0.3f, 0.3f));
    }
}
