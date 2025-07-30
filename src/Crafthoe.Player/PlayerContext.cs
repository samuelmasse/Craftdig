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
    DimensionChunkGeneratedEvent chunkGeneratedEvent,
    DimensionChunkRenderScheduler chunkRenderScheduler,
    PlayerGlw gl,
    PlayerPerspective perspective,
    PlayerCamera camera,
    PlayerControls controls)
{
    private readonly Stopwatch chunkReqWatch = Stopwatch.StartNew();
    private readonly Stopwatch sectionReqWatch = Stopwatch.StartNew();
    private readonly int far = 24;
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
        else
        {
            metrics.SectionMetric.Start();

            sectionReqWatch.Restart();
            bool req;
            do req = RequestSections();
            while (req && sectionReqWatch.ElapsedMilliseconds < 1);

            metrics.SectionMetric.End();
        }

        chunkRenderScheduler.Tick();
        chunkGeneratedEvent.Reset();

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

        var cloc = new Vector2i((int)camera.Offset.X, (int)camera.Offset.Z) / SectionSize;

        metrics.RenderMetric.Start();

        for (int dy = -far; dy <= far; dy++)
        {
            for (int dx = -far; dx <= far; dx++)
            {
                var ncloc = cloc + (dx, dy);
                var chunk = chunks[ncloc].GetValueOrDefault();
                var rendered = chunk.ChunkRendered();
                if (rendered?.Count > 0)
                {
                    foreach (var z in rendered)
                    {
                        var nsloc = new Vector3i(ncloc.X, ncloc.Y, z);
                        if (sections.TryGet(nsloc, out var section) &&
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
            chunkGeneratedEvent.Add(best.Value);
            chunkReqWatch.Restart();
            metrics.ChunkMetric.End();
        }
    }

    private bool RequestSections()
    {
        var sloc = new Vector3i((int)camera.Offset.X, (int)camera.Offset.Z, (int)camera.Offset.Y) / ChunkSize;

        Entity bestChunk = default;
        float bestChunkDist = float.PositiveInfinity;

        for (int dy = -far; dy <= far; dy++)
        {
            for (int dx = -far; dx <= far; dx++)
            {
                var cloc = sloc.Xy + (dx, dy);
                var chunk = chunks[cloc].GetValueOrDefault();
                if (chunk.ChunkUnrendered()?.Count > 0)
                {
                    float dist = Vector2.DistanceSquared(cloc, sloc.Xy);
                    if (dist < bestChunkDist)
                    {
                        bestChunk = chunk;
                        bestChunkDist = dist;
                    }
                }
            }
        }

        var unrendered = bestChunk.ChunkUnrendered();
        if (unrendered == null)
            return false;

        Entity bestSection = default;
        float bestSectionDist = float.PositiveInfinity;

        foreach (var sz in unrendered)
        {
            var nsloc = new Vector3i(bestChunk.ChunkLocation(), sz);

            if (sections.TryGet(nsloc, out var section))
            {
                float dist = Vector3.DistanceSquared(nsloc, sloc);
                if (dist < bestSectionDist)
                {
                    bestSection = section;
                    bestSectionDist = dist;
                }
            }
        }

        if (!bestSection.IsSection())
            return false;

        sectionMesher.Render(bestSection.SectionLocation());

        var mesh = bestSection.SectionTerrainMesh();
        if (sectionMesher.Vertices.Length > 0)
            meshTransferer.Transfer(positionColorTextureProgram3D, sectionMesher.Vertices, ref mesh);
        else meshTransferer.Free(ref mesh);
        bestSection.SectionTerrainMesh(mesh);
        sectionMesher.Reset();

        unrendered.Remove(bestSection.SectionLocation().Z);

        var rendered = bestChunk.ChunkRendered();
        if (rendered == null)
        {
            rendered = [];
            bestChunk.ChunkRendered(rendered);
        }

        rendered.Add(bestSection.SectionLocation().Z);
        return true;
    }
}
