namespace Crafthoe.Player.Frontend;

[Player]
public class PlayerRenderer(
    RootCanvas canvas,
    RootQuadIndexBuffer quadIndexBuffer,
    RootBackbuffer backbuffer,
    ModuleFaceAtlas blockAtlas,
    WorldTick tick,
    DimensionSharedVertexBuffer svb,
    DimensionBlockProgram blockProgram,
    DimensionChunks chunks,
    DimensionSections sections,
    DimensionMetrics metrics,
    DimensionDrawDistance drawDistance,
    DimensionSectionSharedVertexArray sectionSharedVertexArray,
    PlayerGlw gl,
    PlayerPerspective perspective,
    PlayerCamera camera,
    PlayerEnt ent,
    PlayerSelected selected)
{
    public void Render()
    {
        var sky = new Vector3(84, 145, 255);
        backbuffer.Clear(new Vector4(sky / 0xFF, 1));
        camera.ComputeVectors();
        perspective.ComputeMatrix(canvas.Size, camera);
        selected.Render();

        gl.Viewport(canvas.Size);
        gl.Enable(EnableCap.DepthTest);
        gl.DepthFunc(DepthFunction.Less);
        gl.Enable(EnableCap.CullFace);
        gl.CullFace(TriangleFace.Back);

        gl.UseProgram(blockProgram.Id);
        blockProgram.View = perspective.View;
        blockProgram.Projection = perspective.Projection;
        blockAtlas.Bind(blockProgram.SamplerTexture);

        var pos = Vector3d.Lerp(ent.Ent.PrevPosition(), ent.Ent.Position(), (float)tick.Alpha);
        var cloc = pos.ToLoc().Xy.ToCloc();
        (pos.Y, pos.Z) = (pos.Z, pos.Y);

        metrics.RenderMetric.Start();
        gl.BindVertexArray(sectionSharedVertexArray.Vao);

        for (int dy = -drawDistance.Far; dy <= drawDistance.Far; dy++)
        {
            for (int dx = -drawDistance.Far; dx <= drawDistance.Far; dx++)
            {
                var ncloc = cloc + (dx, dy);

                var delta = Vector2i.Abs(cloc - ncloc);
                var dist = delta.X + delta.Y;
                if (dist > drawDistance.Far)
                    continue;

                if (!chunks.TryGet(ncloc, out var chunk))
                    continue;

                for (int i = 0; i < chunk.Rendered().Count; i++)
                {
                    var z = chunk.Rendered()[chunk.Rendered().Keys[i]];
                    var nsloc = new Vector3i(ncloc.X, ncloc.Y, z);
                    if (!sections.TryGet(nsloc, out var section) || section.TerrainMesh().Count <= 0)
                        continue;

                    blockProgram.Offset = (Vector3)(nsloc.Swizzle() * SectionSize - pos);

                    var mesh = section.TerrainMesh();
                    int addr = (int)svb.Addr(mesh.Alloc);

                    GL.DrawElementsBaseVertex(
                        PrimitiveType.Triangles,
                        quadIndexBuffer.IndexCount(mesh.Count),
                        DrawElementsType.UnsignedInt,
                        0,
                        addr / BlockVertex.Size);
                }
            }
        }

        gl.UnbindVertexArray();
        metrics.RenderMetric.End();

        blockAtlas.Unbind(blockProgram.SamplerTexture);
        gl.UnuseProgram();

        gl.ResetCullFace();
        gl.Disable(EnableCap.CullFace);
        gl.ResetDepthFunc();
        gl.Disable(EnableCap.DepthTest);
        gl.ResetViewport();
    }
}
