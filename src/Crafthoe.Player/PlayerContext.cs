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
    DimensionGenerator generator,
    DimensionSectionMesher sectionMesher,
    DimensionMeshTransferer meshTransferer,
    DimensionsMetrics metrics,
    PlayerGlw gl,
    PlayerPerspective perspective,
    PlayerCamera camera,
    PlayerControls controls)
{
    private readonly Stopwatch chunkReqWatch = Stopwatch.StartNew();
    private readonly Stopwatch sectionReqWatch = Stopwatch.StartNew();
    private readonly int far = 32;
    private Texture texture = null!;
    private bool gentype = false;

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
    }

    public void Render()
    {
        if (gentype)
            RequestChunks();
        else RequestSections();

        gentype = !gentype;

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

        var cloc = new Vector3i((int)camera.Offset.X, (int)camera.Offset.Z, (int)camera.Offset.Y) / ChunkSize;

        metrics.RenderMetric.Start();

        for (int dy = -far; dy <= far; dy++)
        {
            for (int dx = -far; dx <= far; dx++)
            {
                for (int dz = -cloc.Z; dz < (HeightSize / SectionSize) - cloc.Z; dz++)
                {
                    var nsloc = cloc + (dx, dy, dz);
                    if (sections.TryGet(nsloc, out var section) &&
                        section.SectionTerrainGenerated() &&
                        section.SectionTerrainMesh().Count > 0)
                    {
                        var mesh = section.SectionTerrainMesh();
                        gl.BindVertexArray(mesh.Vao);
                        gl.DrawElements(BeginMode.Triangles, quadIndexBuffer.IndexCount(mesh.Count), DrawElementsType.UnsignedInt, 0);
                        gl.UnbindVertexArray();
                    }
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

    private void RequestChunks()
    {
        if (chunkReqWatch.ElapsedMilliseconds < 4)
            return;

        var cloc = new Vector2i((int)camera.Offset.X, (int)camera.Offset.Z) / SectionSize;

        Vector2i? best = null;
        float bestDist = 0;

        for (int dy = -far; dy <= far; dy++)
        {
            for (int dx = -far; dx <= far; dx++)
            {
                var ncloc = cloc + (dx, dy);
                if (chunks[ncloc] == null)
                {
                    float dist = Vector2.DistanceSquared(ncloc, cloc);
                    if (best == null || dist < bestDist)
                    {
                        best = ncloc;
                        bestDist = dist;
                    }
                }
            }
        }

        if (best != null)
        {
            metrics.ChunkMetric.Start();
            chunks.Alloc(best.Value);
            generator.Generate(best.Value);
            chunkReqWatch.Restart();
            metrics.ChunkMetric.End();
        }
    }

    private void RequestSections()
    {
        if (sectionReqWatch.ElapsedMilliseconds < 1)
            return;

        var sloc = new Vector3i((int)camera.Offset.X, (int)camera.Offset.Z, (int)camera.Offset.Y) / ChunkSize;

        Entity? best = null;
        float bestDist = 0;

        for (int dy = -far; dy <= far; dy++)
        {
            for (int dx = -far; dx <= far; dx++)
            {
                for (int dz = -sloc.Z; dz < (HeightSize / SectionSize) - sloc.Z; dz++)
                {
                    var nsloc = sloc + (dx, dy, dz);
                    if (sections.TryGet(nsloc, out var section) && !section.SectionTerrainGenerated())
                    {
                        float dist = Vector3.DistanceSquared(nsloc, sloc);
                        if (best == null || dist < bestDist)
                        {
                            best = section;
                            bestDist = dist;
                        }
                    }
                }
            }
        }

        if (best != null)
        {
            metrics.SectionMetric.Start();
            sectionMesher.Render(best.Value.SectionLocation());
            metrics.SectionMetric.End();

            var mesh = best.Value.SectionTerrainMesh();
            if (sectionMesher.Vertices.Length > 0)
                meshTransferer.Transfer(positionColorTextureProgram3D, sectionMesher.Vertices, ref mesh);
            else meshTransferer.Free(ref mesh);
            best.Value.SectionTerrainMesh(mesh);
            best.Value.SectionTerrainGenerated(true);
            sectionMesher.Reset();
            sectionReqWatch.Restart();
            metrics.SectionMetric.End();
        }
    }
}
