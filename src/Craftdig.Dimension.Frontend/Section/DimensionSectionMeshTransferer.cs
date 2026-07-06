namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionSectionMeshTransferer(RootQuadIndexBuffer quadIndexBuffer, DimensionSharedVertexBuffer svb, DimensionGl gl)
{
    public SectionMesh Transfer(ReadOnlySpan<BlockVertex> vertices, SectionMesh dst)
    {
        if (vertices.Length == 0)
            return Free(dst);

        quadIndexBuffer.EnsureCapacity(vertices.Length);

        svb.Alloc(ref dst.Alloc, BlockVertex.Size, vertices.Length * BlockVertex.Size);

        int addr = (int)svb.Addr(dst.Alloc);
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, svb.Vbo);
        gl.BufferSubData(GlBufferTarget.ArrayBuffer, addr, vertices);
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);

        dst.Count = vertices.Length;
        return dst;
    }

    public SectionMesh Free(SectionMesh dst)
    {
        svb.Free(dst.Alloc);
        return default;
    }
}
