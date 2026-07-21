namespace Craftdig.Player.Frontend;

[Player]
public class PlayerViewModelVertexArray
{
    public GlBufferHandle Vbo { get; }
    public GlVertexArrayHandle Vao { get; }

    public PlayerViewModelVertexArray(
        RootQuadIndexBuffer quadIndexBuffer,
        DimensionBlockProgram blockProgram,
        PlayerGl gl)
    {
        Vbo = gl.GenBuffer();
        Vao = gl.GenVertexArray();
        gl.BindVertexArray(Vao);
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, Vbo);
        gl.BindBuffer(GlBufferTarget.ElementArrayBuffer, quadIndexBuffer.Id);
        gl.BufferData(
            GlBufferTarget.ArrayBuffer,
            PlayerViewModelMesher.MaxVertices * BlockVertex.Size,
            0,
            GlBufferUsage.DynamicDraw);
        blockProgram.SetAttributes();
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);
        gl.UnbindVertexArray();

        quadIndexBuffer.EnsureCapacity(PlayerViewModelMesher.MaxVertices);
    }
}
