namespace Crafthoe.Player;

[Player]
public class PlayerContext(
    RootMouse mouse,
    RootCanvas canvas,
    RootQuadIndexBuffer quadIndexBuffer,
    RootPngs pngs,
    AppFiles files,
    DimensionBlockProgram blockProgram,
    DimensionChunks chunks,
    DimensionSections sections,
    DimensionMetrics metrics,
    DimensionChunkRequester chunkRequester,
    PlayerGlw gl,
    PlayerPerspective perspective,
    PlayerCamera camera,
    PlayerControls controls,
    PlayerEntity entity)
{
    private Texture texArray = null!;

    public void Load()
    {
        var image = pngs[files["Textures/Noise.png"]];

        texArray = new Texture(gl, (image.Size.X, image.Size.Y), TextureTarget.Texture2DArray)
        {
            MagFilter = TextureMagFilter.Nearest,
            MinFilter = TextureMinFilter.NearestMipmapLinear,
            WrapS = TextureWrapMode.Repeat,
            WrapT = TextureWrapMode.Repeat
        };

        texArray.Bind();
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.TexImage3D(TextureTarget.Texture2DArray,
            0,
            PixelInternalFormat.Rgba,
            (int)texArray.Size.X,
            (int)texArray.Size.Y,
            0xFF,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            0,
            sizeof(byte),
            4);

        var pixels = new (byte, byte, byte, byte)[image.Pixels.Length];
        image.Pixels.CopyTo(pixels);
        GL.TexSubImage3D(
            TextureTarget.Texture2DArray,
            0, 0, 0, 0,
            (int)texArray.Size.X, (int)texArray.Size.Y, 1, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

        gl.ResetActiveTexture();
        texArray.Unbind();
        texArray.GenerateMipmap();

        entity.Entity.Position() = (15, 15, 100);
    }

    public void Update(double time)
    {
        float speed = (float)(time * 10);

        if (controls.CameraFast.Run())
            speed *= 10;

        var offset = entity.Entity.Position();
        (offset.Y, offset.Z) = (offset.Z, offset.Y);

        if (controls.CameraFront.Run())
            offset += camera.Front * speed;
        if (controls.CameraLeft.Run())
            offset -= camera.Right * speed;
        if (controls.CameraBack.Run())
            offset -= camera.Front * speed;
        if (controls.CameraRight.Run())
            offset += camera.Right * speed;

        if (controls.CameraUp.Run())
            offset.Y += speed;
        if (controls.CameraDown.Run())
            offset.Y -= speed;

        (offset.Y, offset.Z) = (offset.Z, offset.Y);

        mouse.Track = true;
        camera.Rotate(-mouse.Delta / 300);
        camera.PreventBackFlipsAndFrontFlips();
        entity.Entity.Position(offset);
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

        gl.UseProgram(blockProgram.Id);
        blockProgram.View = perspective.View;
        blockProgram.Projection = perspective.Projection;
        texArray.Bind(blockProgram.SamplerTexture);

        var cloc = (Vector2i)(entity.Entity.Position().Xy / SectionSize);
        var pos = entity.Entity.Position();
        (pos.Y, pos.Z) = (pos.Z, pos.Y);

        metrics.RenderMetric.Start();

        for (int dy = -chunkRequester.Far; dy <= chunkRequester.Far; dy++)
        {
            for (int dx = -chunkRequester.Far; dx <= chunkRequester.Far; dx++)
            {
                var ncloc = cloc + (dx, dy);

                var delta = Vector2i.Abs(cloc - ncloc);
                var dist = delta.X + delta.Y;
                if (dist > chunkRequester.Far)
                    continue;

                if (!chunks.TryGet(ncloc, out var chunk))
                    continue;

                for (int i = 0; i < chunk.Rendered().Count; i++)
                {
                    var z = chunk.Rendered()[chunk.Rendered().Keys[i]];
                    var nsloc = new Vector3i(ncloc.X, ncloc.Y, z);
                    if (!sections.TryGet(nsloc, out var section) || section.TerrainMesh().Count <= 0)
                        continue;

                    blockProgram.Offset = (Vector3)(new Vector3i(nsloc.X, nsloc.Z, nsloc.Y) * SectionSize - pos);

                    var mesh = section.TerrainMesh();
                    gl.BindVertexArray(mesh.Vao);
                    gl.DrawElements(BeginMode.Triangles,
                        quadIndexBuffer.IndexCount(mesh.Count), DrawElementsType.UnsignedInt, 0);
                    gl.UnbindVertexArray();
                }
            }
        }

        metrics.RenderMetric.End();

        texArray.Unbind(blockProgram.SamplerTexture);
        gl.UnuseProgram();

        gl.ResetCullFace();
        gl.Disable(EnableCap.CullFace);
        gl.ResetDepthFunc();
        gl.Disable(EnableCap.DepthTest);
        gl.ResetViewport();
    }
}
