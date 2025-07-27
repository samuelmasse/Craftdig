namespace Crafthoe.Player;

[Player]
public class PlayerContext(
    RootMouse mouse,
    RootCanvas canvas,
    RootQuadIndexBuffer quadIndexBuffer,
    RootPositionColorTextureProgram3D positionColorTextureProgram3D,
    RootCube cube,
    RootPngs pngs,
    AppFiles files,
    PlayerGlw gl,
    PlayerPerspective perspective,
    PlayerCamera camera,
    PlayerControls controls)
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

        var noise = new FastNoiseLite();
        var vertices = new List<PositionColorTextureVertex>();

        for (int z = -256; z < 256; z++)
        {
            for (int x = -256; x < 256; x++)
            {
                int y = (int)(noise.GetNoise(x, z) * 15);

                vertices.Add(new(cube.Top.Quad.TopLeft + (x, y, z), (1, 1, 1), (0, 1)));
                vertices.Add(new(cube.Top.Quad.TopRight + (x, y, z), (1, 1, 1), (1, 1)));
                vertices.Add(new(cube.Top.Quad.BottomLeft + (x, y, z), (1, 1, 1), (0, 0)));
                vertices.Add(new(cube.Top.Quad.BottomRight + (x, y, z), (1, 1, 1), (1, 0)));
            }
        }

        vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);
        gl.BindBuffer(BufferTarget.ArrayBuffer, gl.GenBuffer());
        gl.BindBuffer(BufferTarget.ElementArrayBuffer, quadIndexBuffer.Id);
        gl.BufferData(BufferTarget.ArrayBuffer, vertices.ToArray().AsSpan(), BufferUsageHint.StaticDraw);
        positionColorTextureProgram3D.SetAttributes();
        gl.UnbindVertexArray();
        gl.UnbindBuffer(BufferTarget.ArrayBuffer);
        gl.UnbindBuffer(BufferTarget.ElementArrayBuffer);

        camera.Offset = (0, 20, 0);
        quadIndexBuffer.EnsureCapacity(vertices.Count);
        count = quadIndexBuffer.IndexCount(vertices.Count);
    }

    public void Update(double time)
    {
        float speed = (float)(time * 10);

        if (controls.CameraFront.Run())
            camera.Offset += camera.Front * speed;
        if (controls.CameraLeft.Run())
            camera.Offset -= camera.Right * speed;
        if (controls.CameraBack.Run())
            camera.Offset -= camera.Front * speed;
        if (controls.CameraRight.Run())
            camera.Offset += camera.Right * speed;

        if (controls.CameraUp.Run())
            camera.Offset.Y += speed;
        if (controls.CameraDown.Run())
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
