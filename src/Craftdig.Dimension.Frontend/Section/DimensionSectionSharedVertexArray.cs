namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionSectionSharedVertexArray
{
    private readonly GlVertexArrayHandle vao;

    public GlVertexArrayHandle Vao => vao;

    public DimensionSectionSharedVertexArray(
        RootQuadIndexBuffer quadIndexBuffer,
        DimensionGl gl,
        DimensionSharedVertexBuffer svb,
        DimensionBlockProgram blockProgram)
    {
        vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, svb.Vbo);
        gl.BindBuffer(GlBufferTarget.ElementArrayBuffer, quadIndexBuffer.Id);
        blockProgram.SetAttributes();
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);
        gl.UnbindVertexArray();
    }
}
