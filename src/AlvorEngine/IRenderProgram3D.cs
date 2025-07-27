namespace AlvorEngine;

public interface IRenderProgram3D : IRenderProgram
{
    Matrix4 View { set; }
    Matrix4 Projection { set; }
}
