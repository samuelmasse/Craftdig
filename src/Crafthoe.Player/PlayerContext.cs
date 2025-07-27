namespace Crafthoe.Player;

[Player]
public class PlayerContext(
    RootMouse mouse,
    RootKeyboard keyboard,
    RootCanvas canvas,
    RootQuadIndexBuffer quadIndexBuffer,
    RootPositionColorTextureProgram3D positionColorTextureProgram3D,
    RootCube cube,
    RootPngs pngs,
    AppFiles files,
    PlayerGlw gl,
    PlayerPerspective perspective,
    PlayerCamera camera)
{
    private Texture texture = null!;
    private int vao;
    private int count;

    public void Load()
    {
        var image = pngs[files["Textures/Noise.png"]];
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
    }

    public void Render()
    {
        camera.ComputeVectors();
        perspective.ComputeMatrix(canvas.Size, camera);

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
}
