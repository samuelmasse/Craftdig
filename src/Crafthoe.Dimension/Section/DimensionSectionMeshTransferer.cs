namespace Crafthoe.Dimension;

[Dimension]
public class DimensionSectionMeshTransferer(RootQuadIndexBuffer quadIndexBuffer, DimensionSharedVertexBuffer svb, DimensionGlw gl)
{
    public void Transfer(ReadOnlySpan<BlockVertex> vertices, ref SectionMesh dst)
    {
        if (vertices.Length == 0)
        {
            Free(ref dst);
            return;
        }

        quadIndexBuffer.EnsureCapacity(vertices.Length);

        svb.Alloc(ref dst.Alloc, BlockVertex.Size, vertices.Length * BlockVertex.Size);

        int addr = (int)svb.Addr(dst.Alloc);
        gl.BindBuffer(BufferTarget.ArrayBuffer, svb.Vbo);
        gl.BufferSubData(BufferTarget.ArrayBuffer, addr, vertices);
        gl.UnbindBuffer(BufferTarget.ArrayBuffer);

        dst.Count = vertices.Length;
    }

    public void Free(ref SectionMesh dst)
    {
        svb.Free(dst.Alloc);
        dst = default;
    }
}
