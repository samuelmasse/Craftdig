namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionSectionSharedVertexArray
{
    private readonly int vao;

    public int Vao => vao;

    public DimensionSectionSharedVertexArray(
        RootQuadIndexBuffer quadIndexBuffer,
        DimensionGlw gl,
        DimensionSharedVertexBuffer svb,
        DimensionBlockProgram blockProgram)
    {
        vao = gl.GenVertexArray(nameof(DimensionSectionSharedVertexArray));
        gl.BindVertexArray(vao);
        gl.BindBuffer(BufferTarget.ArrayBuffer, svb.Vbo);
        gl.BindBuffer(BufferTarget.ElementArrayBuffer, quadIndexBuffer.Id);
        blockProgram.SetAttributes();
        gl.UnbindVertexArray();
        gl.UnbindBuffer(BufferTarget.ArrayBuffer);
        gl.UnbindBuffer(BufferTarget.ElementArrayBuffer);
    }
}
