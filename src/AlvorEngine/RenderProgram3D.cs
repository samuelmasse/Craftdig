namespace AlvorEngine;

public abstract class RenderProgram3D<T> : RenderProgram<T>, IRenderProgram3D where T : IVertex
{
    private readonly int matView;
    private readonly int matProjection;

    public Matrix4 View
    {
        set => GL.UniformMatrix4(matView, true, ref value);
    }

    public Matrix4 Projection
    {
        set => GL.UniformMatrix4(matProjection, true, ref value);
    }

    protected RenderProgram3D(Glw gl, string vertCode, string fragCode) : base(gl, vertCode, fragCode)
    {
        matView = GL.GetUniformLocation(Id, nameof(matView));
        matProjection = GL.GetUniformLocation(Id, nameof(matProjection));
    }
}
