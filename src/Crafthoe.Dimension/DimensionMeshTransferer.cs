namespace Crafthoe.Dimension;

[Dimension]
public class DimensionMeshTransferer(RootQuadIndexBuffer quadIndexBuffer, DimensionGlw gl)
{
    public void Transfer<T>(IRenderProgram program, ReadOnlySpan<T> vertices, ref VaoVboCount dst) where T : unmanaged, IVertex
    {
        if (dst.Vao == 0)
            dst.Vao = gl.GenVertexArray();

        if (dst.Vbo == 0)
            dst.Vbo = gl.GenBuffer();

        gl.BindVertexArray(dst.Vao);
        gl.BindBuffer(BufferTarget.ArrayBuffer, dst.Vbo);
        gl.BindBuffer(BufferTarget.ElementArrayBuffer, quadIndexBuffer.Id);
        gl.BufferData(BufferTarget.ArrayBuffer, vertices, BufferUsageHint.StaticDraw);
        program.SetAttributes();
        gl.UnbindVertexArray();
        gl.UnbindBuffer(BufferTarget.ArrayBuffer);
        gl.UnbindBuffer(BufferTarget.ElementArrayBuffer);

        quadIndexBuffer.EnsureCapacity(vertices.Length);
        dst.Count = vertices.Length;
    }

    public void Free(ref VaoVboCount dst)
    {
        if (dst.Vbo != 0)
            gl.DeleteBuffer(dst.Vbo);

        if (dst.Vao != 0)
            gl.DeleteVertexArray(dst.Vao);

        dst = default;
    }
}
