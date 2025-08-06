namespace Crafthoe.Player;

[Player]
public class PlayerContext(
    RootMouse mouse,
    RootKeyboard keyboard,
    RootCanvas canvas,
    RootQuadIndexBuffer quadIndexBuffer,
    RootBackbuffer backbuffer,
    ModuleBlockAtlas blockAtlas,
    ModuleEntities entities,
    WorldTick tick,
    DimensionAir air,
    DimensionBlocks blocks,
    DimensionBlockProgram blockProgram,
    DimensionChunks chunks,
    DimensionSections sections,
    DimensionMetrics metrics,
    DimensionChunkRequester chunkRequester,
    DimensionRigidBag rigidBag,
    PlayerScope scope,
    PlayerGlw gl,
    PlayerPerspective perspective,
    PlayerCamera camera,
    PlayerControls controls,
    PlayerEntity entity,
    PlayerSelected selected)
{
    private Ent[] buildableBlocks = [];
    private Ent hand;

    public PlayerScope Scope => scope;

    public void Load()
    {
        entity.Entity.Position() = (15, 0, 100);
        rigidBag.Add((EntMut)entity.Entity);

        var blocks = new List<Ent>();
        foreach (var ent in entities.Span)
        {
            if (ent.IsBlock() && ent.IsBuildable())
                blocks.Add(ent);
        }

        buildableBlocks = [.. blocks];
    }

    public void Tick()
    {
        ref var vel = ref entity.Entity.Velocity();
        vel = vel.Swizzle();

        float speed = 0.05f;

        if (controls.CameraUp.Run())
            vel.Y += speed * 3;
        if (controls.CameraDown.Run())
            vel.Y -= speed * 3;

        if (controls.CameraFast.Run())
            speed *= 2f;

        if (controls.CameraFront.Run())
            vel += camera.Front * speed;
        if (controls.CameraLeft.Run())
            vel -= camera.Right * speed;
        if (controls.CameraBack.Run())
            vel -= camera.Front * speed;
        if (controls.CameraRight.Run())
            vel += camera.Right * speed;

        vel = vel.Swizzle();
    }

    public void Update(double time)
    {
        mouse.Track = true;
        camera.Rotate(-mouse.Delta / 300);
        camera.PreventBackFlipsAndFrontFlips();

        for (int i = 0; i < 9; i++)
        {
            var key = Keys.D1 + i;
            if (keyboard.IsKeyPressed(key))
            {
                if (i < buildableBlocks.Length)
                    hand = buildableBlocks[i];
                else hand = default;
            }
        }

        if (selected.Loc != null)
        {
            if (mouse.IsMainPressed())
                blocks.TrySet(selected.Loc.Value, air.Block);
            if (selected.Normal != null && hand.IsBuildable() && mouse.IsSecondaryPressed())
                blocks.TrySet(selected.Loc.Value + selected.Normal.Value, hand);
        }
    }

    public void Render()
    {
        var sky = new Vector3(103, 150, 239);
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

        var pos = Vector3d.Lerp(entity.Entity.PrevPosition(), entity.Entity.Position(), (float)tick.Alpha);
        var cloc = pos.ToLoc().Xy.ToCloc();
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

                    blockProgram.Offset = (Vector3)(nsloc.Swizzle() * SectionSize - pos);

                    var mesh = section.TerrainMesh();
                    gl.BindVertexArray(mesh.Vao);
                    gl.DrawElements(BeginMode.Triangles,
                        quadIndexBuffer.IndexCount(mesh.Count), DrawElementsType.UnsignedInt, 0);
                    gl.UnbindVertexArray();
                }
            }
        }

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
