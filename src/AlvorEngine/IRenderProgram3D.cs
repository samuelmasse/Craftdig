namespace AlvorEngine;

internal interface IRenderProgram3D : IRenderProgram
{
    public Matrix4 View { set; }
    public Matrix4 Projection { set; }
}
