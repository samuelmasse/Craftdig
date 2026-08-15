namespace Craftdig;

[Player]
public class PlayerRigidRenderer(
    RootQuadIndexBuffer quadIndexBuffer,
    WorldTick tick,
    DimensionRemoteInterpolation remoteInterpolation,
    DimensionSharedVertexBuffer svb,
    DimensionGl gl,
    DimensionSectionSharedVertexArray sectionSharedVertexArray,
    DimensionChunkRigids chunkRigids,
    PlayerTestCubeMesher testCubeMesher,
    PlayerBodyMesher playerBodyMesher,
    PlayerBlockParticleMesher blockParticleMesher)
{
    private readonly List<BlockVertex> vertices = [];
    private int alloc;

    public void Mesh(Ent chunk, Vec3d origin)
    {
        foreach (var rigid in chunkRigids[chunk.Cloc])
        {
            if (!rigid.IsLoaded)
                continue;

            bool remotePlayer = rigid.IsRemote && rigid.IsPlayer;
            if (!remotePlayer && !rigid.IsTestCube)
                continue;

            var worldPosition = rigid.IsRemote
                ? remoteInterpolation.Position(rigid)
                : Vec3d.Lerp(rigid.PrevPosition, rigid.Position, (float)tick.Alpha);

            if (remotePlayer)
            {
                playerBodyMesher.Mesh(
                    vertices,
                    worldPosition,
                    remoteInterpolation.LookAt(rigid),
                    (Vec3)origin);
            }
            else
            {
                testCubeMesher.Mesh(
                    vertices,
                    worldPosition,
                    origin,
                    rigid.TestCubeMaterial.Faces,
                    rigid.TestCubeSize);
            }
        }
    }

    public void MeshBlockParticles(Vec3d origin) =>
        blockParticleMesher.Mesh(vertices, origin);

    public void Render()
    {
        if (vertices.Count == 0)
            return;

        quadIndexBuffer.EnsureCapacity(vertices.Count);
        svb.Alloc(ref alloc, BlockVertex.Size, vertices.Count * BlockVertex.Size);

        int address = (int)svb.Addr(alloc);
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, svb.Vbo);
        gl.BufferSubData(GlBufferTarget.ArrayBuffer, address, CollectionsMarshal.AsSpan(vertices));
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);

        gl.BindVertexArray(sectionSharedVertexArray.Vao);
        gl.DrawElementsBaseVertex(
            GlPrimitiveType.Triangles,
            quadIndexBuffer.IndexCount(vertices.Count),
            GlDrawElementsType.UnsignedInt,
            0,
            address / BlockVertex.Size);
        gl.UnbindVertexArray();

        vertices.Clear();
    }
}
