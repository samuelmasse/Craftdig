namespace Crafthoe.Player;

[Player]
public class PlayerContext(
    RootMouse mouse,
    RootCanvas canvas,
    RootQuadIndexBuffer quadIndexBuffer,
    RootPositionColorTextureProgram3D positionColorTextureProgram3D,
    RootPngs pngs,
    AppFiles files,
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
    private Texture texture = null!;

    public Vector3 Position => (camera.Offset.X, camera.Offset.Z, camera.Offset.Y);

    public void Load()
    {
        var image = pngs[files["Textures/Noise.png"]];
        texture = new Texture2D(gl, image.Size)
        {
            PixelsMipmap = image.Pixels.Span,
            MagFilter = TextureMagFilter.Nearest,
            MinFilter = TextureMinFilter.NearestMipmapLinear
        };

        camera.Offset = (0, 80, 0);
    }

    public void Update(double time)
    {
        float speed = (float)(time * 10);

        if (controls.CameraFast.Run())
            speed *= 10;

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
        entity.Entity.Position((camera.Offset.X, camera.Offset.Z, camera.Offset.Y));
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

        var cloc = new Vector2i((int)camera.Offset.X, (int)camera.Offset.Z) / SectionSize;

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

                var chunk = chunks[ncloc].GetValueOrDefault();
                var rendered = chunk.ChunkRendered();
                if (rendered == null)
                    continue;

                foreach (var z in rendered)
                {
                    var nsloc = new Vector3i(ncloc.X, ncloc.Y, z);
                    if (!sections.TryGet(nsloc, out var section) || section.SectionTerrainMesh().Count <= 0)
                        continue;

                    var mesh = section.SectionTerrainMesh();
                    gl.BindVertexArray(mesh.Vao);
                    gl.DrawElements(BeginMode.Triangles,
                        quadIndexBuffer.IndexCount(mesh.Count), DrawElementsType.UnsignedInt, 0);
                    gl.UnbindVertexArray();
                }
            }
        }

        metrics.RenderMetric.End();

        texture.Unbind(positionColorTextureProgram3D.SamplerTexture);
        gl.UnuseProgram();

        gl.ResetCullFace();
        gl.Disable(EnableCap.CullFace);
        gl.ResetDepthFunc();
        gl.Disable(EnableCap.DepthTest);
        gl.ResetViewport();
    }
}
