namespace Craftdig.Dimension.Frontend;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct BlockVertex(Vec3 Position, Vec3 Lighting, Vec3 TexCoord) : IVertex
{
    public static readonly int Size = Marshal.SizeOf<BlockVertex>();

    public static void SetAttributes(GlLayer gl)
    {
        gl.VertexAttribPointer(0, 3, GlVertexAttribPointerType.Float, false, 9 * sizeof(float), 0);
        gl.EnableVertexAttribArray(0);

        gl.VertexAttribPointer(1, 3, GlVertexAttribPointerType.Float, false, 9 * sizeof(float), 3 * sizeof(float));
        gl.EnableVertexAttribArray(1);

        gl.VertexAttribPointer(2, 3, GlVertexAttribPointerType.Float, false, 9 * sizeof(float), 6 * sizeof(float));
        gl.EnableVertexAttribArray(2);
    }
}
