namespace AlvorEngine;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct PositionColorVertex(Vector3 Position, Vector3 Color) : IVertex
{
    public static void SetAttributes(Glw gl)
    {
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        gl.EnableVertexAttribArray(0);

        gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        gl.EnableVertexAttribArray(1);
    }
}
