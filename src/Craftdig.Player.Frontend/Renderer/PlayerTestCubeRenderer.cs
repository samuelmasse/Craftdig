namespace Craftdig.Player.Frontend;

[Player]
public class PlayerTestCubeRenderer(
    RootCube cube,
    RootQuadIndexBuffer quadIndexBuffer,
    WorldTick tick,
    DimensionSharedVertexBuffer svb,
    DimensionGlw gl,
    DimensionSectionSharedVertexArray sectionSharedVertexArray,
    DimensionChunkRigids chunkRigids)
{
    private readonly List<BlockVertex> vertices = [];
    private int alloc;

    public void Mesh(Ent chunk, Vector3d origin)
    {
        var rigids = chunkRigids[chunk.Cloc];

        foreach (var rigid in rigids)
        {
            if (!rigid.IsTestCube)
                continue;

            var block = rigid.TestCubeMaterial;
            var faces = block.Faces;
            var size = rigid.TestCubeSize;
            var pos = Vector3d.Lerp(rigid.PrevPosition, rigid.Position, (float)tick.Alpha).Swizzle();

            AddQuad(cube.Front.Quad, 0.8f, faces.Front.FaceIndex);
            AddQuad(cube.Back.Quad, 0.8f, faces.Back.FaceIndex);

            AddQuad(cube.Left.Quad, 0.6f, faces.Left.FaceIndex);
            AddQuad(cube.Right.Quad, 0.6f, faces.Right.FaceIndex);

            AddQuad(cube.Top.Quad, 1f, faces.Top.FaceIndex);
            AddQuad(cube.Bottom.Quad, 0.5f, faces.Bottom.FaceIndex);

            void AddQuad(Quad quad, float shadow, int texture)
            {
                var off = pos - origin - new Vector3(size) / 2;
                vertices.Add(new((Vector3)(quad.TopLeft * size + off), Vector3.One * shadow, (0, 1, texture)));
                vertices.Add(new((Vector3)(quad.TopRight * size + off), Vector3.One * shadow, (1, 1, texture)));
                vertices.Add(new((Vector3)(quad.BottomLeft * size + off), Vector3.One * shadow, (0, 0, texture)));
                vertices.Add(new((Vector3)(quad.BottomRight * size + off), Vector3.One * shadow, (1, 0, texture)));
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
        gl.BindBuffer(BufferTarget.ArrayBuffer, svb.Vbo);
        gl.BufferSubData(BufferTarget.ArrayBuffer, addr, CollectionsMarshal.AsSpan(vertices));
        gl.UnbindBuffer(BufferTarget.ArrayBuffer);

        gl.BindVertexArray(sectionSharedVertexArray.Vao);
        GL.DrawElementsBaseVertex(
            PrimitiveType.Triangles,
            quadIndexBuffer.IndexCount(vertices.Count),
            DrawElementsType.UnsignedInt,
            0,
            addr / BlockVertex.Size);
        gl.UnbindVertexArray();

        vertices.Clear();
    }
}
