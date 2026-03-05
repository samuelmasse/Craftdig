namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionSectionMeshTransferer(RootQuadIndexBuffer quadIndexBuffer, DimensionSharedVertexBuffer svb, DimensionGlw gl)
{
    public SectionMesh Transfer(ReadOnlySpan<BlockVertex> vertices, SectionMesh dst)
    {
        if (vertices.Length == 0)
            return Free(dst);

        quadIndexBuffer.EnsureCapacity(vertices.Length);

        svb.Alloc(ref dst.Alloc, BlockVertex.Size, vertices.Length * BlockVertex.Size);

        int addr = (int)svb.Addr(dst.Alloc);
        gl.BindBuffer(BufferTarget.ArrayBuffer, svb.Vbo);
        gl.BufferSubData(BufferTarget.ArrayBuffer, addr, vertices);
        gl.UnbindBuffer(BufferTarget.ArrayBuffer);

        dst.Count = vertices.Length;
        return dst;
    }

    public SectionMesh Free(SectionMesh dst)
    {
        svb.Free(dst.Alloc);
        return default;
    }
}
