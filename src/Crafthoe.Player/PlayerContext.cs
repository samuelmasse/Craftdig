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
    private Texture texture = null!;
    private int vao;
    private int count;
    private int far = 10;
    private bool gentype = false;
    private readonly Stopwatch chunkReqWatch = Stopwatch.StartNew();
    private readonly Stopwatch sectionReqWatch = Stopwatch.StartNew();

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

        camera.Offset = (0, 80, 0);
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

        var cloc = new Vector3i((int)camera.Offset.X, (int)camera.Offset.Z, (int)camera.Offset.Y) / chunks.Unit;

        metrics.RenderMetric.Start();

        for (int dy = -far; dy <= far; dy++)
        {
            for (int dx = -far; dx <= far; dx++)
            {
                for (int dz = -cloc.Z; dz < (chunks.Unit.Z / sections.Unit.Z) - cloc.Z; dz++)
                {
                    var nsloc = cloc + (dx, dy, dz);
                    if (sections.TryGet(nsloc, out var section) && section.SectionTerrainGenerated())
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

        var cloc = new Vector2i((int)camera.Offset.X, (int)camera.Offset.Z) / chunks.Unit.Xy;

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

        var sloc = new Vector3i((int)camera.Offset.X, (int)camera.Offset.Z, (int)camera.Offset.Y) / chunks.Unit;

        Entity? best = null;
        float bestDist = 0;

        for (int dy = -far; dy <= far; dy++)
        {
            for (int dx = -far; dx <= far; dx++)
            {
                for (int dz = -sloc.Z; dz < (chunks.Unit.Z / sections.Unit.Z) - sloc.Z; dz++)
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
            var mesh = best.Value.SectionTerrainMesh();
            meshTransferer.Transfer(positionColorTextureProgram3D, sectionMesher.Vertices, ref mesh);
            best.Value.SectionTerrainMesh(mesh);
            best.Value.SectionTerrainGenerated(true);
            sectionMesher.Reset();
            sectionReqWatch.Restart();
            metrics.SectionMetric.End();
        }
    }
}
