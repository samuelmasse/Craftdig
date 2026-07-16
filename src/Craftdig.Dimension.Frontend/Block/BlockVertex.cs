namespace Craftdig.Dimension.Frontend;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct BlockVertex(Vec3 Position, Vec3 Lighting, Vec3 TexCoord) : IVertex
{
    public static readonly int Size = Marshal.SizeOf<BlockVertex>();

    public static void SetAttributes(GlLayer gl)
    {
        gl.VertexAttribPointer<Vec3>(0, false, Size, 0);
        gl.EnableVertexAttribArray(0);

        gl.VertexAttribPointer<Vec3>(1, false, Size, 3 * sizeof(float));
        gl.EnableVertexAttribArray(1);

        gl.VertexAttribPointer<Vec3>(2, false, Size, 6 * sizeof(float));
        gl.EnableVertexAttribArray(2);
    }
}
