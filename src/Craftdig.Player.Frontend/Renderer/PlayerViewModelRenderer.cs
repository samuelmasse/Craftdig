namespace Craftdig;

[Player]
public class PlayerViewModelRenderer(
    RootQuadIndexBuffer quadIndexBuffer,
    ModuleFaceAtlas faceAtlas,
    WorldSky sky,
    DimensionLights lights,
    DimensionBlockProgram blockProgram,
    PlayerGl gl,
    PlayerEnt ent,
    PlayerViewBob viewBob,
    PlayerViewModelCamera camera,
    PlayerViewModelPerspective perspective,
    PlayerViewModelVertexArray vertexArray,
    PlayerViewModelAnimation animation,
    PlayerViewModelMesher mesher)
{
    private readonly BlockVertex[] vertices = new BlockVertex[PlayerViewModelMesher.MaxVertices];

    public void Render(Vec2 canvas)
    {
        var levels = lights.Get(ent.Position.ToLoc());
        int count = mesher.Mesh(
            vertices,
            animation.Pose,
            animation.DisplayedItem,
            levels.Sky / (float)LightLevel.Max,
            levels.Block / (float)LightLevel.Max);
        if (count == 0)
            return;

        perspective.ComputeMatrix(canvas, camera);
        viewBob.Apply(ref perspective.View);

        gl.Viewport((Vec2u)canvas);
        gl.ClearDepth(1);
        gl.Clear(GlClearBufferMask.DepthBufferBit);
        gl.ResetClearDepth();
        gl.Enable(GlEnableCap.DepthTest);
        gl.DepthFunc(GlDepthFunction.Less);
        gl.Enable(GlEnableCap.CullFace);
        gl.CullFace(GlTriangleFace.Back);

        gl.UseProgram(blockProgram.Id);
        blockProgram.View = perspective.View;
        blockProgram.Projection = perspective.Projection;
        blockProgram.Offset = default;
        blockProgram.SkyStrength = sky.Strength;
        faceAtlas.Bind(blockProgram.SamplerTexture);

        gl.BindBuffer(GlBufferTarget.ArrayBuffer, vertexArray.Vbo);
        gl.BufferSubData(GlBufferTarget.ArrayBuffer, 0, vertices.AsSpan(0, count));
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);

        gl.BindVertexArray(vertexArray.Vao);
        gl.DrawElements(
            GlPrimitiveType.Triangles,
            quadIndexBuffer.IndexCount(count),
            GlDrawElementsType.UnsignedInt,
            0);
        gl.UnbindVertexArray();

        faceAtlas.Unbind(blockProgram.SamplerTexture);
        gl.UnuseProgram();
        gl.ResetCullFace();
        gl.Disable(GlEnableCap.CullFace);
        gl.ResetDepthFunc();
        gl.Disable(GlEnableCap.DepthTest);
        gl.ResetViewport();
    }
}
