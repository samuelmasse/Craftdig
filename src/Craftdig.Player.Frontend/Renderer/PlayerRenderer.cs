namespace Craftdig.Player.Frontend;

[Player]
public class PlayerRenderer(
    RootQuadIndexBuffer quadIndexBuffer,
    AppRenderDistance renderDistance,
    ModuleFaceAtlas blockAtlas,
    WorldTick tick,
    DimensionSharedVertexBuffer svb,
    DimensionBlockProgram blockProgram,
    DimensionChunks chunks,
    DimensionSections sections,
    DimensionSectionSharedVertexArray sectionSharedVertexArray,
    PlayerMetrics metrics,
    PlayerGl gl,
    PlayerPerspective perspective,
    PlayerCamera camera,
    PlayerEnt ent,
    PlayerTestCubeRenderer testCubeRenderer)
{
    private readonly List<EntMutIdx> schunks = [];
    private readonly List<float> dists = [];

    public void Render(Vec2 canvas)
    {
        var sky = new Vec3(84, 145, 255);

        gl.Viewport(0, 0, (int)canvas.X, (int)canvas.Y);
        var clear = new Vec4(sky / 0xFF, 1);
        gl.ClearColor(clear.X, clear.Y, clear.Z, clear.W);
        gl.ClearDepth(1f);
        gl.Clear(GlClearBufferMask.DepthBufferBit | GlClearBufferMask.ColorBufferBit);
        gl.ResetClearDepth();
        gl.ResetClearColor();

        camera.ComputeVectors();
        perspective.ComputeMatrix(canvas, camera);

        var frustum = Frustum3.CreateFromClipTransform(perspective.Projection * perspective.View);

        gl.Enable(GlEnableCap.DepthTest);
        gl.DepthFunc(GlDepthFunction.Less);
        gl.Enable(GlEnableCap.CullFace);
        gl.CullFace(GlTriangleFace.Back);

        gl.UseProgram(blockProgram.Id);
        blockProgram.View = perspective.View;
        blockProgram.Projection = perspective.Projection;
        blockAtlas.Bind(blockProgram.SamplerTexture);

        var pos = Vec3d.Lerp(ent.PrevPosition, ent.Position, (float)tick.Alpha);
        var cloc = pos.ToLoc().XY.ToCloc();
        pos = pos.Swizzle();

        metrics.RenderMetric.Start();
        gl.BindVertexArray(sectionSharedVertexArray.Vao);

        schunks.Clear();
        dists.Clear();

        for (int dy = -renderDistance.Far; dy <= renderDistance.Far; dy++)
        {
            for (int dx = -renderDistance.Far; dx <= renderDistance.Far; dx++)
            {
                var ncloc = cloc + (dx, dy);

                var delta = Vec2i.Abs(cloc - ncloc);
                var dist = delta.X + delta.Y;
                if (dist > renderDistance.Far)
                    continue;

                if (!chunks.TryGet(ncloc, out var chunk))
                    continue;

                var colMin = (Vec3)(new Vec3i(ncloc.X, ncloc.Y, 0).Swizzle() * SectionSize - pos);
                var colMax = colMin + (SectionSize, HeightSize, SectionSize);
                if (!frustum.Intersects(new Box3(colMin, colMax)))
                    continue;

                float colCenterX = colMin.X + (SectionSize / 2f);
                float colCenterZ = colMin.Z + (SectionSize / 2f);
                float distSq = colCenterX * colCenterX + colCenterZ * colCenterZ;

                schunks.Add(chunk);
                dists.Add(distSq);
            }
        }

        var schunksSpan = CollectionsMarshal.AsSpan(schunks);
        var distsSpan = CollectionsMarshal.AsSpan(dists);
        distsSpan.Sort(schunksSpan);

        foreach (var chunk in schunksSpan)
        {
            int count = chunk.Rendered.Count;
            if (count == 0)
                continue;

            var keys = chunk.Rendered.Keys;
            var values = chunk.Rendered.Values;

            float targetHeightIdx = (float)pos.Y / SectionSize;
            int closestIdx = 0;
            float minDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                float dist = Math.Abs(keys[i] - targetHeightIdx);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestIdx = i;
                }
            }

            // We do a sorted walk from the closest section
            int up = closestIdx;
            int down = closestIdx - 1;

            while (up < count || down >= 0)
            {
                int index;

                if (up >= count)
                    index = down--;
                else if (down < 0)
                    index = up++;
                else
                {
                    float distUp = Math.Abs(keys[up] - targetHeightIdx);
                    float distDown = Math.Abs(keys[down] - targetHeightIdx);
                    index = distUp < distDown ? up++ : down--;
                }

                var z = values[index];
                var nsloc = new Vec3i(chunk.Cloc.X, chunk.Cloc.Y, z);

                if (!sections.TryGet(nsloc, out var section) || section.TerrainMesh.Count <= 0)
                    continue;

                var center = (Vec3)(nsloc.Swizzle() * SectionSize - pos) + new Vec3(SectionSize / 2);
                var min = center - new Vec3(SectionSize / 2);
                var max = center + new Vec3(SectionSize / 2);

                if (!frustum.Intersects(new Box3(min, max)))
                    continue;

                blockProgram.Offset = (Vec3)(nsloc.Swizzle() * SectionSize - pos);

                var mesh = section.TerrainMesh;
                int addr = (int)svb.Addr(mesh.Alloc);

                gl.DrawElementsBaseVertex(
                    GlPrimitiveType.Triangles,
                    quadIndexBuffer.IndexCount(mesh.Count),
                    GlDrawElementsType.UnsignedInt,
                    0,
                    addr / BlockVertex.Size);
            }
        }

        foreach (var chunk in schunksSpan)
            testCubeRenderer.Mesh(chunk, pos);

        gl.UnbindVertexArray();
        blockProgram.Offset = default;
        testCubeRenderer.Render();

        metrics.RenderMetric.End();

        blockAtlas.Unbind(blockProgram.SamplerTexture);
        gl.UnuseProgram();

        gl.ResetCullFace();
        gl.Disable(GlEnableCap.CullFace);
        gl.ResetDepthFunc();
        gl.Disable(GlEnableCap.DepthTest);
        gl.ResetViewport();
    }
}
