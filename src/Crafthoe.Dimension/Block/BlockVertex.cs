namespace AlvorEngine;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct BlockVertex(Vector3 Position, Vector3 Color, Vector3 TexCoord) : IVertex
{
    public static void SetAttributes(Glw gl)
    {
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0);
        gl.EnableVertexAttribArray(0);

        gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 3 * sizeof(float));
        gl.EnableVertexAttribArray(1);

        gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 6 * sizeof(float));
        gl.EnableVertexAttribArray(2);
    }
}
