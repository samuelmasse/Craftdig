namespace Craftdig.Player.Frontend;

[Player]
public class PlayerTestCubeRenderer(
    RootCube cube,
    RootQuadIndexBuffer quadIndexBuffer,
    WorldTick tick,
    DimensionSharedVertexBuffer svb,
    DimensionGl gl,
    DimensionSectionSharedVertexArray sectionSharedVertexArray,
    DimensionChunkRigids chunkRigids)
{
    private readonly List<BlockVertex> vertices = [];
    private int alloc;

    public void Mesh(Ent chunk, Vec3d origin)
    {
        foreach (var rigid in chunkRigids[chunk.Cloc])
        {
            if (!rigid.IsTestCube || !rigid.IsLoaded)
                continue;

            var block = rigid.TestCubeMaterial;
            var faces = block.Faces;
            var size = rigid.TestCubeSize;
            var pos = Vec3d.Lerp(rigid.PrevPosition, rigid.Position, (float)tick.Alpha).Swizzle();

            AddQuad(cube.Front.Quad, 0.8f, faces.Front.FaceIndex);
            AddQuad(cube.Back.Quad, 0.8f, faces.Back.FaceIndex);

            AddQuad(cube.Left.Quad, 0.6f, faces.Left.FaceIndex);
            AddQuad(cube.Right.Quad, 0.6f, faces.Right.FaceIndex);

            AddQuad(cube.Top.Quad, 1f, faces.Top.FaceIndex);
            AddQuad(cube.Bottom.Quad, 0.5f, faces.Bottom.FaceIndex);

            void AddQuad(Quad3 quad, float shadow, int texture)
            {
                var off = pos - origin - new Vec3(size) / 2;
                vertices.Add(new((Vec3)(quad.TopLeft * size + off), Vec3.One * shadow, (0, 1, texture)));
                vertices.Add(new((Vec3)(quad.TopRight * size + off), Vec3.One * shadow, (1, 1, texture)));
                vertices.Add(new((Vec3)(quad.BottomLeft * size + off), Vec3.One * shadow, (0, 0, texture)));
                vertices.Add(new((Vec3)(quad.BottomRight * size + off), Vec3.One * shadow, (1, 0, texture)));
            }
        }
    }

    public void Render()
    {
        if (vertices.Count == 0)
            return;

        quadIndexBuffer.EnsureCapacity(vertices.Count);

        svb.Alloc(ref alloc, BlockVertex.Size, vertices.Count * BlockVertex.Size);

        int addr = (int)svb.Addr(alloc);
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, svb.Vbo);
        gl.BufferSubData(GlBufferTarget.ArrayBuffer, addr, CollectionsMarshal.AsSpan(vertices));
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);

        gl.BindVertexArray(sectionSharedVertexArray.Vao);
        gl.DrawElementsBaseVertex(
            GlPrimitiveType.Triangles,
            quadIndexBuffer.IndexCount(vertices.Count),
            GlDrawElementsType.UnsignedInt,
            0,
            addr / BlockVertex.Size);
        gl.UnbindVertexArray();

        vertices.Clear();
    }
}
